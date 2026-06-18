# externals/

Third-party SVG **test corpora** that the CodeBrix.SkiaSvg test suite renders
and pixel-compares against. None of this is authored by CodeBrix, and **none of
it ships in the NuGet package** — it is used only by the test projects.

CodeBrix.SkiaSvg is a fork of **Svg.Skia v4.2.0** (see `../THIRD-PARTY-NOTICES.txt`).
These corpora are pinned to the exact commits that Svg.Skia v4.2.0 referenced as
git submodules, so the bundled reference PNGs match the comparison thresholds in
the test code.

| Folder | Upstream | Pinned commit | License |
|---|---|---|---|
| `resvg/` | https://github.com/wieslawsoltes/resvg | `a739aef5d01360ec238c886bc50674f31458df00` | MPL-2.0 (`resvg/LICENSE.txt`) |
| `W3C_SVG_11_TestSuite/` | https://github.com/wieslawsoltes/W3C_SVG_11_TestSuite | `b6937c121ce24c09bf204bc51b73d73233a18354` | W3C Document License (`.../images/copyright-documents-19990405.html`; per-file `Copyright … W3C` headers) |

## How the tests find these

- `tests/.../resvgTests.cs` → `externals/resvg/tests/{svg,png}/<name>.{svg,png}`
- `tests/.../W3CTestSuiteTests.cs` → `externals/W3C_SVG_11_TestSuite/W3C_SVG_11_TestSuite/{svg,png}/<name>.{svg,png}`

If these folders are absent, the resvg/W3C tests fail with missing-file errors.
(They are additionally gated to macOS via the `[OSXTheory]` attribute, because
the reference PNGs were generated on the macOS Skia rasterizer.)

## These are committed to the repo

The corpora above are checked in, so a normal clone already has them — there is
**no fetch or setup step** on macOS, Linux, or anywhere else. Just build and run
the tests.

If the corpora ever need to be regenerated, re-download each upstream repository
at the pinned commit listed in the table above and lay it out at the same path.
