// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SkiaSvg.TypefaceProviders;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

public partial class SkiaModel
{
    private const int TypefaceCacheLimit = 512;
    private const int ResolvedTypefaceCacheLimit = 512;
    private const int PositionedTextCacheRefTrimThreshold = 1024;

    private sealed class PictureReferenceEqualityComparer : IEqualityComparer<ShimSkiaSharp.SKPicture>
    {
        public static readonly PictureReferenceEqualityComparer Instance = new();

        public bool Equals(ShimSkiaSharp.SKPicture x, ShimSkiaSharp.SKPicture y) => ReferenceEquals(x, y);

        public int GetHashCode(ShimSkiaSharp.SKPicture obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private readonly struct TypefaceKey : IEquatable<TypefaceKey>
    {
        public TypefaceKey(
            string familyName,
            SkiaSharp.SKFontStyleWeight weight,
            SkiaSharp.SKFontStyleWidth width,
            SkiaSharp.SKFontStyleSlant slant)
        {
            FamilyName = familyName;
            Weight = weight;
            Width = width;
            Slant = slant;
        }

        public string FamilyName { get; }
        public SkiaSharp.SKFontStyleWeight Weight { get; }
        public SkiaSharp.SKFontStyleWidth Width { get; }
        public SkiaSharp.SKFontStyleSlant Slant { get; }

        public bool Equals(TypefaceKey other)
        {
            return string.Equals(FamilyName, other.FamilyName, StringComparison.Ordinal)
                && Weight == other.Weight
                && Width == other.Width
                && Slant == other.Slant;
        }

        public override bool Equals(object obj)
        {
            return obj is TypefaceKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = FamilyName?.GetHashCode() ?? 0;
                hash = (hash * 397) ^ (int)Weight;
                hash = (hash * 397) ^ (int)Width;
                hash = (hash * 397) ^ (int)Slant;
                return hash;
            }
        }
    }

    private readonly struct FontSignature : IEquatable<FontSignature>
    {
        public FontSignature(SkiaSharp.SKFont font)
        {
            TypefaceHandle = font.Typeface?.Handle ?? IntPtr.Zero;
            Size = font.Size;
            ScaleX = font.ScaleX;
            SkewX = font.SkewX;
            Edging = font.Edging;
            Subpixel = font.Subpixel;
            Embolden = font.Embolden;
        }

        public IntPtr TypefaceHandle { get; }
        public float Size { get; }
        public float ScaleX { get; }
        public float SkewX { get; }
        public SkiaSharp.SKFontEdging Edging { get; }
        public bool Subpixel { get; }
        public bool Embolden { get; }

        public bool Equals(FontSignature other)
        {
            return TypefaceHandle == other.TypefaceHandle
                && Size.Equals(other.Size)
                && ScaleX.Equals(other.ScaleX)
                && SkewX.Equals(other.SkewX)
                && Edging == other.Edging
                && Subpixel == other.Subpixel
                && Embolden == other.Embolden;
        }

        public override bool Equals(object obj)
        {
            return obj is FontSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = TypefaceHandle.GetHashCode();
                hash = (hash * 397) ^ Size.GetHashCode();
                hash = (hash * 397) ^ ScaleX.GetHashCode();
                hash = (hash * 397) ^ SkewX.GetHashCode();
                hash = (hash * 397) ^ (int)Edging;
                hash = (hash * 397) ^ Subpixel.GetHashCode();
                hash = (hash * 397) ^ Embolden.GetHashCode();
                return hash;
            }
        }
    }

    private sealed class PositionedTextCache
    {
        public PositionedTextCache(FontSignature signature, SkiaSharp.SKTextBlob textBlob)
        {
            Signature = signature;
            TextBlob = textBlob;
        }

        public FontSignature Signature { get; }
        public SkiaSharp.SKTextBlob TextBlob { get; }
    }

    private readonly ConcurrentDictionary<TypefaceKey, SkiaSharp.SKTypeface> _typefaceCache = new();
    private readonly ConcurrentDictionary<TypefaceKey, SkiaSharp.SKTypeface> _resolvedTypefaceCache = new();
    private readonly object _positionedTextCacheLock = new();
    private readonly object _pictureCacheLock = new();
    private ConditionalWeakTable<DrawTextBlobCanvasCommand, PositionedTextCache> _positionedTextCache = new();
    private readonly List<WeakReference<SkiaSharp.SKTextBlob>> _positionedTextCacheRefs = new();
    private readonly Dictionary<ShimSkiaSharp.SKPicture, SkiaSharp.SKPicture> _pictureCache = new(PictureReferenceEqualityComparer.Instance);
    private IList<ITypefaceProvider> _providerStateList;
    private int _providerStateHash;

    internal void RegisterCachedPicture(ShimSkiaSharp.SKPicture picture, SkiaSharp.SKPicture skPicture)
    {
        lock (_pictureCacheLock)
        {
            _pictureCache[picture] = skPicture;
        }
    }

    internal void UnregisterCachedPicture(ShimSkiaSharp.SKPicture picture)
    {
        if (picture is null)
        {
            return;
        }

        lock (_pictureCacheLock)
        {
            _ = _pictureCache.Remove(picture);
        }
    }

    internal bool TryGetCachedPicture(ShimSkiaSharp.SKPicture picture, out SkiaSharp.SKPicture skPicture)
    {
        lock (_pictureCacheLock)
        {
            return _pictureCache.TryGetValue(picture, out skPicture!);
        }
    }

    internal void ClearCachedPictures()
    {
        lock (_pictureCacheLock)
        {
            _pictureCache.Clear();
        }
    }
}
