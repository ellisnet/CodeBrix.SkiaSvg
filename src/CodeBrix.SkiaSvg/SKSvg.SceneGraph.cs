using System;
using System.Collections.Generic;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SvgParse;
using CodeBrix.SkiaSvg.Model.Services;

namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

public partial class SKSvg
{
    private SvgSceneDocument _retainedSceneGraph;
    private bool _retainedSceneGraphDirty = true;

    /// <summary>Gets the retained scene graph, building it if necessary.</summary>
    public SvgSceneDocument RetainedSceneGraph
    {
        get
        {
            _ = TryEnsureRetainedSceneGraph(out var sceneDocument);
            return sceneDocument;
        }
    }

    /// <summary>Gets a value indicating whether a retained scene graph is available.</summary>
    public bool HasRetainedSceneGraph => RetainedSceneGraph is not null;

    /// <summary>Ensures that a retained scene graph is compiled and available.</summary>
    /// <param name="sceneDocument">When this method returns, contains the scene document.</param>
    /// <returns><c>true</c> if the scene graph is available; otherwise, <c>false</c>.</returns>
    public bool TryEnsureRetainedSceneGraph(out SvgSceneDocument sceneDocument)
    {
        SvgDocument sourceDocument;

        lock (Sync)
        {
            if (!_retainedSceneGraphDirty)
            {
                sceneDocument = _retainedSceneGraph;
                return sceneDocument is not null;
            }

            sourceDocument = _animatedDocument ?? SourceDocument;
        }

        if (sourceDocument is null)
        {
            lock (Sync)
            {
                _retainedSceneGraph = null;
                _retainedSceneGraphDirty = false;
                sceneDocument = null;
            }

            return false;
        }

        if (!SvgSceneRuntime.TryCompile(sourceDocument, AssetLoader, IgnoreAttributes, GetStandaloneViewport(), out var compiledSceneDocument))
        {
            lock (Sync)
            {
                _retainedSceneGraph = null;
                _retainedSceneGraphDirty = false;
                sceneDocument = null;
            }

            return false;
        }

        lock (Sync)
        {
            _retainedSceneGraph = compiledSceneDocument;
            _retainedSceneGraphDirty = false;
            sceneDocument = compiledSceneDocument;
        }

        return true;
    }

    /// <summary>Creates a drawing model from the retained scene graph.</summary>
    /// <returns>The picture model, or <c>null</c> if no scene graph is available.</returns>
    public SKPicture CreateRetainedSceneGraphModel()
    {
        return RetainedSceneGraph is { } sceneDocument
            ? sceneDocument.CreateModel()
            : null;
    }

    /// <summary>Creates a SkiaSharp picture from the retained scene graph.</summary>
    /// <returns>The SkiaSharp picture, or <c>null</c> if no scene graph is available.</returns>
    public SkiaSharp.SKPicture CreateRetainedSceneGraphPicture()
    {
        var model = CreateRetainedSceneGraphModel();
        return model is null ? null : SkiaModel.ToSKPicture(model);
    }

    /// <summary>Tries to get a retained scene node by address key.</summary>
    /// <param name="addressKey">The element address key.</param>
    /// <param name="node">When this method returns, contains the matching node.</param>
    /// <returns><c>true</c> if a matching node was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneNode(string addressKey, out SvgSceneNode node)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            return sceneDocument.TryGetNode(addressKey, out node);
        }

        node = null;
        return false;
    }

    /// <summary>Tries to get a retained scene node for the specified SVG element.</summary>
    /// <param name="element">The SVG element.</param>
    /// <param name="node">When this method returns, contains the matching node.</param>
    /// <returns><c>true</c> if a matching node was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneNode(SvgElement element, out SvgSceneNode node)
    {
        if (element is null)
        {
            throw new System.ArgumentNullException(nameof(element));
        }

        return TryGetRetainedSceneNode(SvgSceneCompiler.TryGetElementAddressKey(element) ?? string.Empty, out node);
    }

    /// <summary>Tries to get all retained scene nodes matching the specified address key.</summary>
    /// <param name="addressKey">The element address key.</param>
    /// <param name="nodes">When this method returns, contains the matching nodes.</param>
    /// <returns><c>true</c> if matching nodes were found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneNodes(string addressKey, out IReadOnlyList<SvgSceneNode> nodes)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            return sceneDocument.TryGetNodes(addressKey, out nodes);
        }

        nodes = System.Array.Empty<SvgSceneNode>();
        return false;
    }

    /// <summary>Tries to get all retained scene nodes for the specified SVG element.</summary>
    /// <param name="element">The SVG element.</param>
    /// <param name="nodes">When this method returns, contains the matching nodes.</param>
    /// <returns><c>true</c> if matching nodes were found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneNodes(SvgElement element, out IReadOnlyList<SvgSceneNode> nodes)
    {
        if (element is null)
        {
            throw new System.ArgumentNullException(nameof(element));
        }

        return TryGetRetainedSceneNodes(SvgSceneCompiler.TryGetElementAddressKey(element) ?? string.Empty, out nodes);
    }

    /// <summary>Tries to get a retained scene node by its SVG element identifier.</summary>
    /// <param name="id">The SVG element identifier.</param>
    /// <param name="node">When this method returns, contains the matching node.</param>
    /// <returns><c>true</c> if a matching node was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneNodeById(string id, out SvgSceneNode node)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            return sceneDocument.TryGetNodeById(id, out node);
        }

        node = null;
        return false;
    }

    /// <summary>Tries to get a retained scene resource by address key.</summary>
    /// <param name="addressKey">The resource address key.</param>
    /// <param name="resource">When this method returns, contains the matching resource.</param>
    /// <returns><c>true</c> if a matching resource was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneResource(string addressKey, out SvgSceneResource resource)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            return sceneDocument.TryGetResource(addressKey, out resource);
        }

        resource = null;
        return false;
    }

    /// <summary>Tries to get a retained scene resource by its SVG element identifier.</summary>
    /// <param name="id">The SVG element identifier.</param>
    /// <param name="resource">When this method returns, contains the matching resource.</param>
    /// <returns><c>true</c> if a matching resource was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRetainedSceneResourceById(string id, out SvgSceneResource resource)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            return sceneDocument.TryGetResourceById(id, out resource);
        }

        resource = null;
        return false;
    }

    /// <summary>Applies a mutation to the retained scene graph for the specified element.</summary>
    /// <param name="element">The SVG element that was mutated.</param>
    /// <param name="changedAttributes">The names of the changed attributes, or <c>null</c>.</param>
    /// <returns>The result of the mutation operation.</returns>
    public SvgSceneMutationResult ApplyRetainedSceneMutation(SvgElement element, IReadOnlyCollection<string> changedAttributes = null)
    {
        return TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null
            ? sceneDocument.ApplyMutation(element, changedAttributes)
            : new SvgSceneMutationResult(false, 0, 0);
    }

    /// <summary>Applies a mutation to the retained scene graph for the element at the specified address key.</summary>
    /// <param name="addressKey">The element address key.</param>
    /// <param name="changedAttributes">The names of the changed attributes, or <c>null</c>.</param>
    /// <returns>The result of the mutation operation.</returns>
    public SvgSceneMutationResult ApplyRetainedSceneMutation(string addressKey, IReadOnlyCollection<string> changedAttributes = null)
    {
        return TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null
            ? sceneDocument.ApplyMutation(addressKey, changedAttributes)
            : new SvgSceneMutationResult(false, 0, 0);
    }

    /// <summary>Applies a mutation to the retained scene graph for the element with the specified identifier.</summary>
    /// <param name="id">The SVG element identifier.</param>
    /// <param name="changedAttributes">The names of the changed attributes, or <c>null</c>.</param>
    /// <returns>The result of the mutation operation.</returns>
    public SvgSceneMutationResult ApplyRetainedSceneMutationById(string id, IReadOnlyCollection<string> changedAttributes = null)
    {
        return TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null
            ? sceneDocument.ApplyMutationById(id, changedAttributes)
            : new SvgSceneMutationResult(false, 0, 0);
    }

    /// <summary>Creates a drawing model for a specific scene node from the retained scene graph.</summary>
    /// <param name="node">The scene node to render.</param>
    /// <param name="clip">An optional clip rectangle.</param>
    /// <returns>The picture model, or <c>null</c> if no scene graph is available.</returns>
    public SKPicture CreateRetainedSceneNodeModel(SvgSceneNode node, SKRect? clip = null)
    {
        if (!TryEnsureRetainedSceneGraph(out var sceneDocument) || sceneDocument is null)
        {
            return null;
        }

        return sceneDocument.CreateNodeModel(node, clip);
    }

    /// <summary>Creates a SkiaSharp picture for a specific scene node from the retained scene graph.</summary>
    /// <param name="node">The scene node to render.</param>
    /// <param name="clip">An optional clip rectangle.</param>
    /// <returns>The SkiaSharp picture, or <c>null</c> if no scene graph is available.</returns>
    public SkiaSharp.SKPicture CreateRetainedSceneNodePicture(SvgSceneNode node, SKRect? clip = null)
    {
        var model = CreateRetainedSceneNodeModel(node, clip);
        return model is null ? null : SkiaModel.ToSKPicture(model);
    }

    /// <summary>Creates a drawing model for the specified SVG element from the retained scene graph.</summary>
    /// <param name="element">The SVG element to render.</param>
    /// <param name="clip">An optional clip rectangle.</param>
    /// <returns>The picture model, or <c>null</c> if no matching node is found.</returns>
    public SKPicture CreateRetainedSceneModel(SvgElement element, SKRect? clip = null)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return TryGetRetainedSceneNode(element, out var node) && node is not null
            ? CreateRetainedSceneNodeModel(node, clip)
            : null;
    }

    /// <summary>Creates a SkiaSharp picture for the specified SVG element from the retained scene graph.</summary>
    /// <param name="element">The SVG element to render.</param>
    /// <param name="clip">An optional clip rectangle.</param>
    /// <returns>The SkiaSharp picture, or <c>null</c> if no matching node is found.</returns>
    public SkiaSharp.SKPicture CreateRetainedScenePicture(SvgElement element, SKRect? clip = null)
    {
        var model = CreateRetainedSceneModel(element, clip);
        return model is null ? null : SkiaModel.ToSKPicture(model);
    }

    private void InvalidateRetainedSceneGraph()
    {
        lock (Sync)
        {
            _retainedSceneGraphDirty = true;
            _retainedSceneGraph = null;
        }
    }

    private bool TryPrepareRetainedSceneGraphForAnimationFrame(SvgAnimationFrameState frameState, SvgAnimationFrameState previousFrameState, out SvgSceneDocument sceneDocument)
    {
        SvgDocument currentDocument;

        lock (Sync)
        {
            currentDocument = _animatedDocument ?? SourceDocument;
        }

        sceneDocument = null;
        if (currentDocument is null)
        {
            return false;
        }

        if (!TryEnsureRetainedSceneGraph(out sceneDocument) || sceneDocument is null)
        {
            return false;
        }

        if (!ReferenceEquals(sceneDocument.SourceDocument, currentDocument))
        {
            return TryRebuildRetainedSceneGraphForCurrentDocument(currentDocument, out sceneDocument);
        }

        foreach (var dirtyAttribute in frameState.EnumerateDirtyAttributes(previousFrameState))
        {
            if (!sceneDocument.TryResolveElement(dirtyAttribute.TargetAddress.Key, out var targetElement) || targetElement is null)
            {
                return TryRebuildRetainedSceneGraphForCurrentDocument(currentDocument, out sceneDocument);
            }

            var result = sceneDocument.ApplyMutation(targetElement, new[] { dirtyAttribute.AttributeName });
            if (!result.Succeeded)
            {
                return TryRebuildRetainedSceneGraphForCurrentDocument(currentDocument, out sceneDocument);
            }
        }

        foreach (var removedAttribute in frameState.EnumerateRemovedAttributes(previousFrameState))
        {
            if (!sceneDocument.TryResolveElement(removedAttribute.TargetAddress.Key, out var targetElement) || targetElement is null)
            {
                return TryRebuildRetainedSceneGraphForCurrentDocument(currentDocument, out sceneDocument);
            }

            var result = sceneDocument.ApplyMutation(targetElement, new[] { removedAttribute.AttributeName });
            if (!result.Succeeded)
            {
                return TryRebuildRetainedSceneGraphForCurrentDocument(currentDocument, out sceneDocument);
            }
        }

        return true;
    }

    private bool TryRebuildRetainedSceneGraphForCurrentDocument(SvgDocument currentDocument, out SvgSceneDocument sceneDocument)
    {
        DisableAnimationLayerCaching();

        if (!SvgSceneRuntime.TryCompile(currentDocument, AssetLoader, IgnoreAttributes, GetStandaloneViewport(), out var compiledSceneDocument) ||
            compiledSceneDocument is null)
        {
            InvalidateRetainedSceneGraph();
            sceneDocument = null;
            return false;
        }

        if (!ReferenceEquals(compiledSceneDocument.SourceDocument, currentDocument))
        {
            InvalidateRetainedSceneGraph();
            sceneDocument = null;
            return false;
        }

        lock (Sync)
        {
            _retainedSceneGraph = compiledSceneDocument;
            _retainedSceneGraphDirty = false;
            sceneDocument = compiledSceneDocument;
        }

        return true;
    }
}
