# Visualization Split Plan

Status: approved canonical plan
Target visualization repository: https://github.com/eosfor/PSGraphView.git
Primary architectural decision: use an object-based boundary between graph core and visualization. Do not use serialized export as the main internal contract.
Local scaffold status: initialized at `/Users/andrei/repo/PSGraphView` with a working `PSGraphView.Vega` seed project covering the force-directed, adjacency-matrix, and tree-layout Vega paths plus the extracted DSM Vega node/edge data builder, a `PSGraphView.Dsm` seed project covering extracted DSM SVG rendering, and a `PSGraphView.Msagl` seed project covering the MSAGL fast-incremental and Sugiyama SVG paths.

## Recommendation
Split the system along the line of graph domain versus visualization implementations. Keep graph models, graph algorithms, DSM, PowerShell graph operations, and textual/interchange exports in PSGraph. Move visualization-specific services, renderers, adapters, and layout engines to PSGraphView. Treat GraphML as an interchange format owned by PSGraph, not as the required internal transport between PSGraph and PSGraphView.

This plan assumes an evolutionary migration with backward compatibility for Export-Graph. The implementation strategy is to introduce adapters and service boundaries first, then move format-specific implementations one slice at a time.

## Boundary
What stays in PSGraph:
- Graph model and graph semantics
- Graph algorithms and traversal
- DSM logic and cmdlets
- Public PowerShell cmdlets and compatibility façade behavior
- Neutral contracts or DTOs, if needed for the boundary
- Textual and interchange exports, including Graphviz DOT and GraphML
- Textual DSM export, including matrix/text output and any DSM DOT output that remains purely textual

What moves to PSGraphView:
- Vega rendering and view adapters
- MSAGL layout and SVG generation
- Visualization-specific rendering services and exporters
- Any renderer integrations that produce visual output rather than textual graph representations
- DSM visualization helpers and renderers, including SVG and Vega-oriented view generation now mixed into `/Users/andrei/repo/PSGraph/DSM/DsmView.cs`

Preferred DSM handoff:
- The target design is direct handoff from `PSGraph` to `PSGraphView` for DSM visualization work
- `Export-DSM` or another thin orchestration layer in `PSGraph` should pass the DSM object or a neutral DSM visualization contract to `PSGraphView`
- Compatibility bridges inside `DsmView` are transitional only and are not the intended end state

Where GraphML lives:
- GraphML remains an interchange format owned by PSGraph
- GraphML must not become the mandatory internal handoff between PSGraph and PSGraphView
- GraphML should stay neutral and non-visualization-owned

Where DOT lives:
- Graphviz DOT remains a textual export owned by PSGraph
- DOT is not a migration target for PSGraphView unless it becomes an input to a separate visualization pipeline

## Target State UX
These examples describe the intended PowerShell user experience after the visualization split is complete. They are target-state examples, not a promise that the cmdlet names already exist.

Target-state principles:
- `PSGraph` continues to create and analyze graphs and DSMs
- `PSGraphView` becomes the visualization-facing module
- Textual exports remain in `PSGraph`, while visual rendering flows through `PSGraphView`

Graph to force-directed HTML:
```powershell
New-Graph |
  Add-Edge -From api -To db -PassThru |
  Add-Edge -From api -To cache -PassThru |
  Export-GraphView -Renderer Vega.ForceDirected -As Html -Path ./graph.html
```

Graph to Sugiyama SVG:
```powershell
New-Graph |
  Add-Edge -From build -To test -PassThru |
  Add-Edge -From test -To deploy -PassThru |
  Export-GraphView -Renderer Msagl.Sugiyama -As Svg -Path ./pipeline.svg
```

Graph to fast-incremental SVG with style tuning:
```powershell
New-Graph |
  Add-Edge -From A -To B -PassThru |
  Add-Edge -From A -To C -PassThru |
  Add-Edge -From C -To D -PassThru |
  Export-GraphView -Renderer Msagl.FastIncremental -As Svg -StylePreset Spread -ShowLabels -Path ./graph.svg
```

Plain DSM to matrix SVG:
```powershell
New-DSM -Graph $g |
  Export-DSMView -Renderer Dsm.MatrixSvg -Path ./dsm.svg
```

Clustered DSM to Vega HTML:
```powershell
Start-DSMClustering -Dsm (New-DSM -Graph $g) |
  Export-DSMView -Renderer Dsm.VegaMatrix -As Html -Path ./dsm.html
```

Sequenced DSM to Vega JSON:
```powershell
Start-DSMSequencing -Dsm (New-DSM -Graph $g) |
  Export-DSMView -Renderer Dsm.VegaMatrix -As Json -Path ./dsm.json
```

Text export stays in PSGraph:
```powershell
New-DSM -Graph $g | Export-DSM -Format TEXT -Path ./dsm.csv
New-Graph | Export-Graph -Format Graphviz -Path ./graph.dot
```

Implications for API shape:
- A separate visualization-facing surface such as `Export-GraphView` and `Export-DSMView` is preferred over continuing to add visual renderers into `Export-Graph`
- `PSGraph` remains responsible for building `PsBidirectionalGraph` and `IDsm` objects
- `PSGraphView` should accept those objects directly, or a neutral visualization contract when direct object handoff becomes impractical

## Target Cmdlet Contract
This section captures the minimal intended PowerShell contract for the visualization module. It is a target-state contract sketch, not a claim that the commands already exist.

### Export-GraphView
Purpose:
- Render a `PsBidirectionalGraph` into a visual format owned by `PSGraphView`

Input:
- `-Graph <PsBidirectionalGraph>`

Core parameters:
- `-Renderer <string>`
- `-As <string>`
- `-Path <string>` optional

Suggested renderer values:
- `Vega.ForceDirected`
- `Vega.AdjacencyMatrix`
- `Vega.TreeLayout`
- `Msagl.FastIncremental`
- `Msagl.Sugiyama`

Suggested output values for `-As`:
- `Html`
- `Json`
- `Svg`

Behavior rules:
- If `-Path` is provided, write the rendered output to that path and do not emit the full payload to the pipeline
- If `-Path` is omitted, emit the rendered string payload to the pipeline
- Renderer-specific tuning parameters may exist, but the base contract should stay stable across renderers
- The command should fail explicitly when a renderer/output combination is unsupported

Minimal example shape:
```powershell
Export-GraphView -Graph $graph -Renderer Msagl.Sugiyama -As Svg -Path ./graph.svg
```

### Export-DSMView
Purpose:
- Render an `IDsm` or DSM-derived result into a visual format owned by `PSGraphView`

Input options:
- `-Dsm <IDsm>`
- `-Result <PartitioningResult>`
- `-SequencedDsm <IDsm>`

Core parameters:
- `-Renderer <string>`
- `-As <string>` optional when the renderer has a single natural output
- `-Path <string>` optional

Suggested renderer values:
- `Dsm.MatrixSvg`
- `Dsm.VegaMatrix`

Suggested output values for `-As`:
- `Html`
- `Json`
- `Svg`

Behavior rules:
- The command should accept DSM objects directly rather than requiring callers to manually build a view-model first
- Clustered and sequenced DSM inputs should preserve their ordering/partition semantics into the renderer
- Textual DSM export remains outside this cmdlet and stays in `Export-DSM`
- If `-Path` is omitted, emit the rendered string payload to the pipeline

Minimal example shape:
```powershell
Export-DSMView -Dsm $dsm -Renderer Dsm.MatrixSvg -Path ./dsm.svg
```

### Naming and Boundary Rules
- `Export-Graph` remains the home for textual graph exports such as `Graphviz` and `GraphML`
- `Export-DSM` remains the home for textual DSM export such as `TEXT`
- `Export-GraphView` and `Export-DSMView` are preferred names because they describe visualization without overloading existing export commands further
- `PSGraphView` should own renderer selection and renderer-specific option handling
- `PSGraph` should own object construction, algorithms, and compatibility-oriented orchestration only where needed

### Parameter Design Guidance
- Prefer a stable `-Renderer` string contract over a growing enum in `PSGraph`
- Keep `-As` orthogonal to `-Renderer` when the same renderer can emit multiple payload shapes
- Avoid forcing users to pass module-internal DTOs such as `GraphView` or a DSM visual DTO from PowerShell
- Add renderer-specific parameter sets only when they materially improve discoverability and do not fragment the base contract

## Coupling Today
- Graphviz-specific state has already been removed from the core graph models and replaced with neutral render properties.
- Graph visualization renderers now delegate from `PSGraph` compatibility cmdlets into `PSGraphView` via neutral `GraphView` handoff.
- GraphML and Graphviz DOT remain in `PSGraph` as neutral/textual formats and are no longer part of the migration ambiguity.
- DSM SVG generation no longer routes through `DsmView`; the remaining `DsmView` type is now limited to local text/Graphviz-style helpers rather than visualization renderer bridging.
- `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj` has already dropped the old MSAGL, legacy Vega, and stale `Svg` package references.

## Remaining Technical Debt
- No blocking visualization-split debt remains in `PSGraph` after the compatibility bridge cleanup.
- Future cleanup, if desired, should be optional API simplification rather than renderer extraction.

## Slice Catalog

### Slice VS-01
Status: done
Title: Establish visualization service boundary for Vega
Goal:
- Introduce the smallest practical adapter or service boundary for one Vega export path while preserving Export-Graph compatibility
Scope:
- Start with Vega_ForceDirected or Vega_AdjacencyMatrix only
- Keep current PowerShell API intact
- Prepare code so that this implementation can later move to PSGraphView with minimal friction
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs`
- `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaDataConverter.cs`
- `/Users/andrei/repo/PSGraph/PSGraph.Tests/VegaDataConverterTests.cs`
- `/Users/andrei/repo/PSGraph/PSGraph.Tests/ExportGraphViewCmdletTests.cs`
Definition of done:
- One Vega export path goes through a dedicated service or adapter boundary
- Existing Export-Graph behavior remains compatible for that path
- Relevant tests are updated or added and pass
Implementation notes:
- Introduced `VegaGraphExportService` in `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaGraphExportService.cs` as the first dedicated visualization service boundary.
- Migrated only the `Vega_ForceDirected` path in `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` to call the new service, keeping the public `Export-Graph` surface unchanged.
- Added direct service coverage in `/Users/andrei/repo/PSGraph/PSGraph.Tests/VegaDataConverterTests.cs` while retaining existing cmdlet-level compatibility tests.
Validation:
- `dotnet test PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~VegaDataConverterTests|FullyQualifiedName~ExportGraphViewCmdLetTests"`
- Result: 25 tests passed, 0 failed.

### Slice VS-02
Status: done
Title: Introduce shared visualization contracts
Goal:
- Create the smallest neutral contract surface needed between PSGraph and PSGraphView
Scope:
- Add a shared export context or GraphView DTO only if it clearly reduces coupling
- Avoid dragging rendering-specific types into contracts
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/GraphExportTypes.cs`
- `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj`
- `/Users/andrei/repo/PSGraph/PSGraph.Common/PSGraph.Common.csproj`
Definition of done:
- A neutral contract exists for at least one visualization path or exporter call
- No new Graphviz/MSAGL/Vega-specific type leaks into core contracts
Implementation notes:
- Added a neutral `GraphView` contract in `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/GraphView.cs` with `GraphViewNode` and `GraphViewEdge`, plus `ToGraphView()` as the bridge from `PsBidirectionalGraph`.
- Moved the Vega force-directed mapping in `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaDataConverter.cs` onto the new `GraphView` contract while preserving the existing `PsBidirectionalGraph` extension as a compatibility bridge.
- Updated `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaGraphExportService.cs` and `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` so the extracted Vega path now flows through the neutral contract before rendering.
Validation:
- `dotnet test PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~VegaDataConverterTests|FullyQualifiedName~ExportGraphViewCmdLetTests"`
- Result: 26 tests passed, 0 failed.

### Slice VS-03
Status: done
Title: Remove rendering-specific fields from core models
Goal:
- Stop embedding Graphviz-specific state directly in graph domain objects
Scope:
- Start with PSVertex and PSEdge
- Replace rendering state with neutral metadata or move behavior into adapters
- Preserve compatibility as far as possible
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSVertex.cs`
- `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSEdge.cs`
Definition of done:
- Core models no longer directly depend on Graphviz-specific types
- Existing export paths still work through compatibility bridges or adapters
Implementation notes:
- Removed direct `GraphvizVertex` and `GraphvizEdge` fields from `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSVertex.cs` and `/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSEdge.cs`, replacing them with neutral `RenderProperties` bags.
- Updated `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` so Graphviz export now materializes output attributes from neutral render properties instead of reading Graphviz-specific model state.
- Added regression coverage to confirm label synchronization, cloned render state, and Graphviz DOT formatting still work through the compatibility bridge.
Validation:
- `dotnet test PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~PSVertexTests|FullyQualifiedName~ExportGraphViewCmdLetTests|FullyQualifiedName~VegaDataConverterTests"`
- Result: 34 tests passed, 0 failed.

### Slice VS-04
Status: done
Title: Convert Export-Graph into a façade for moved Vega path
Goal:
- Make Export-Graph a thin orchestration layer for the already extracted Vega path
Scope:
- Limit the change to the path already covered by VS-01
- Do not migrate every format in one shot
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs`
- `/Users/andrei/repo/PSGraph/PSGraph.Tests/ExportGraphViewCmdletTests.cs`
Definition of done:
- Export-Graph delegates one visualization path to a dedicated service/facade instead of hosting the logic inline
Implementation notes:
- Added `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaGraphExportFacade.cs` so the migrated `Vega_ForceDirected` path now has a dedicated façade that routes by export format and delegates to the extracted service.
- Updated `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` so `Export-Graph` now routes the force-directed Vega path straight into the façade, leaving the cmdlet with module-path resolution and I/O orchestration only.
- Added cmdlet-level regression coverage in `/Users/andrei/repo/PSGraph/PSGraph.Tests/ExportGraphViewCmdletTests.cs` to confirm the façade path still writes a valid force-directed Vega JSON file through the public PowerShell surface.
Validation:
- `dotnet test PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~ExportGraphViewCmdLetTests|FullyQualifiedName~VegaDataConverterTests"`
- Result: 28 tests passed, 0 failed.

### Slice VS-05
Status: done
Title: Clarify textual export boundary for GraphML and DOT
Goal:
- Make GraphML an explicit interchange format and DOT an explicit textual export that both stay in PSGraph
Scope:
- Clarify ownership and usage
- Add tests or docs if needed
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ImportGraphCmdlet.cs`
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs`
- `/Users/andrei/repo/PSGraph/docs/Import-Graph.md`
- `/Users/andrei/repo/PSGraph/docs/Export-Graph.md`
Definition of done:
- GraphML usage is explicit and documented as interchange, not internal mandatory transport
- DOT export is explicit and documented as staying in PSGraph rather than moving to PSGraphView
Implementation notes:
- Updated `/Users/andrei/repo/PSGraph/README.md`, `/Users/andrei/repo/PSGraph/docs/Import-Graph.md`, and `/Users/andrei/repo/PSGraph/docs/Export-Graph.md` so GraphML is described as the neutral interchange format owned by `PSGraph`, while Graphviz DOT remains the textual graph export owned by `PSGraph`.
- Clarified the same ownership boundary in secondary docs/examples so visual renderer formats are described as compatibility paths backed by `PSGraphView`, not as replacements for GraphML or DOT.
Validation:
- Documentation-only clarification; no executable surface changed.

### Slice VS-06
Status: done
Title: Extract visual rendering behind service boundaries
Goal:
- Pull visualization-specific rendering code out of core/cmdlet layers behind dedicated services or adapters
Scope:
- Keep current cmdlet parameters intact
- Preserve output compatibility as much as practical
- Include DSM visual-only methods from `/Users/andrei/repo/PSGraph/DSM/DsmView.cs` in scope for migration
- Keep textual DSM export in PSGraph
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs`
- `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.TreeLayout.cs`
- `/Users/andrei/repo/PSGraph/DSM/DsmView.cs`
- `/Users/andrei/repo/PSGraph/docs/Export-DSM.md`
- `/Users/andrei/repo/PSGraph/PSGraph.Tests/ExportGraphViewCmdletTests.cs`
Definition of done:
- MSAGL logic no longer lives directly in the cmdlet orchestration path for the migrated slice
- DSM visualization helpers no longer live in `DsmView` once their migrated slice is extracted
Implementation notes:
- Narrowed the first safe sub-slice to DSM Vega data shaping only.
- Moved the reorderable-matrix node/edge payload builder out of `/Users/andrei/repo/PSGraph/DSM/DsmView.cs` into `/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/DsmNodeAndEdgeViewBuilder.cs`.
- Updated `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/DSM/ExportDSMCmdlet.cs` to use the extracted builder while leaving DSM text export and DSM SVG generation in `PSGraph` for later slices.
- Added a second sub-slice that extracts DSM SVG rendering into `/Users/andrei/repo/PSGraphView/src/PSGraphView.Dsm/DsmSvgExporter.cs` and turns `/Users/andrei/repo/PSGraph/DSM/DsmView.cs` into a compatibility bridge for `ToSvg()` and `ToSvgString()`.
- That bridge is a temporary compatibility step for existing `DsmView` callers; the target follow-up is to let `PSGraph` hand DSM visualization directly to `PSGraphView` without routing through `DsmView`.
- Added a follow-up delegation step so `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` now routes `Vega_AdjacencyMatrix` and `Vega_TreeLayout` through the extracted `PSGraphView.Vega` exporters instead of keeping template assembly inline in `PSGraph`.
- Updated `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/DSM/ExportDSMCmdlet.cs` to call `/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/DsmVegaMatrixExporter.cs`, so the public `Export-DSM` compatibility path now delegates DSM Vega matrix rendering to `PSGraphView` while keeping `TEXT` in `PSGraph`.
- Added a further delegation step so `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` now routes `MSAGL_FASTINCREMENTAL` and `MSAGL_SUGIYAMA` through `/Users/andrei/repo/PSGraphView/src/PSGraphView.Msagl/`, while keeping `MSAGL_MDS` on the legacy local path for now.
- Extended `/Users/andrei/repo/PSGraphView/src/PSGraphView.Msagl/MsaglFastIncrementalOptions.cs` and `/Users/andrei/repo/PSGraphView/src/PSGraphView.Msagl/MsaglFastIncrementalExporter.cs` so the old force-layout tuning knobs from `Export-Graph` still flow into the extracted renderer.
- Added `/Users/andrei/repo/PSGraphView/src/PSGraphView.Msagl/MsaglMdsExporter.cs` and switched `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` to delegate `MSAGL_MDS` there as well, so all MSAGL visualization formats now render through `PSGraphView`.
- Removed the old local MSAGL helper implementation by deleting `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.TreeLayout.cs` after the extracted `PSGraphView.Msagl` path fully covered `MDS`, `FASTINCREMENTAL`, and `SUGIYAMA`.
- Added macOS test-host stability guards in `/Users/andrei/repo/PSGraph/PSGraph.Tests/AssemblyInfo.cs` and `/Users/andrei/repo/PSGraphView/tests/PSGraphView.PowerShell.Tests/AssemblyInfo.cs` because PowerShell-hosting xUnit runs were crashing under parallel execution during provider initialization.
- Removed the temporary DSM SVG compatibility bridge from `/Users/andrei/repo/PSGraph/DSM/DsmView.cs` and `/Users/andrei/repo/PSGraph/DSM/Interface/IDsmView.cs`, leaving `DsmView` responsible only for local text and Graphviz-style helpers.
- Updated `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/DSM/ExportDSMCmdlet.cs` so `Export-DSM` no longer routes through `DsmView` for visualization; `TEXT` is emitted directly from the DSM matrix and Vega rendering continues to delegate to `PSGraphView`.
- Switched `/Users/andrei/repo/PSGraph/PSGraph.Tests/Graph5GraphBasedPartitioningTests.cs` to exercise `/Users/andrei/repo/PSGraphView/src/PSGraphView.Dsm/DsmSvgExporter.cs` directly, then removed the no-longer-needed `PSGraphView.Dsm` reference from `/Users/andrei/repo/PSGraph/DSM/DSM.csproj` and the stale `Svg` package reference from `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj`.
Validation:
- `dotnet build /Users/andrei/repo/PSGraph/PSGraph.Tests/PSGraph.Tests.csproj`
- `dotnet test /Users/andrei/repo/PSGraph/PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~DsmCmdletsBasicTests|FullyQualifiedName~Graph5GraphBasedPartitioningTests"`

### Slice VS-07
Status: done
Title: Reduce visualization dependencies in PSGraph project
Goal:
- Remove or isolate visualization-specific package references from the main PSGraph project after service extraction
Scope:
- Only change dependencies that are no longer needed by migrated slices
- Avoid premature removal that breaks current behavior
Key files:
- `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj`
- `/Users/andrei/repo/PSGraph/PSGraph.Common/PSGraph.Common.csproj`
Definition of done:
- Visualization dependencies are reduced in PSGraph in line with the slices already moved or isolated

Implementation notes:
- Removed direct MSAGL package references from `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj` after the remaining `MSAGL_MDS` compatibility path moved to `/Users/andrei/repo/PSGraphView/src/PSGraphView.Msagl/`.
- Cleaned stale MSAGL-only code and imports from `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` and `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/VegaDataConverter.cs`.
- Reduced `/Users/andrei/repo/PSGraph/PSGraph.Common/PSGraph.Common.csproj` to only the package references it still uses directly (`QuikGraph` and `QuikGraph.Graphviz`), removing stale visualization and unrelated references such as `QuikGraph.MSAGL`, `Svg`, `MathNet.*`, `QuikGraph.Serialization`, `Microsoft.Extensions.Logging`, and `System.Management.Automation`.
- Switched the remaining `Vega_ForceDirected` compatibility path in `/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs` to `/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/VegaForceDirectedExporter.cs`, which let `/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj` drop its runtime reference to `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/PSGraph.Vega.Extensions.csproj`.
- Removed the now-dead runtime façade/service files from `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/` and trimmed `VegaHelper` down to the legacy/test-helper surface that still supports `PSGraph.Tests` template-based assertions.
- Migrated the remaining template-based Vega tests in `/Users/andrei/repo/PSGraph/PSGraph.Tests/VegaDataConverterTests.cs` onto `/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/` exporters and mapping extensions, which let `/Users/andrei/repo/PSGraph/PSGraph.Tests/PSGraph.Tests.csproj` drop its direct dependency on `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/PSGraph.Vega.Extensions.csproj`.
- Removed `/Users/andrei/repo/PSGraph/PSGraph.Vega.Extensions/` from `/Users/andrei/repo/PSGraph/PSGraph.sln` and deleted the legacy project files once all runtime and test references were gone.
- At this point the remaining dependency cleanup is intentionally coupled to VS-06: `Svg` stays only while the DSM compatibility bridge in `DsmView` still exists.
Validation:
- `dotnet build /Users/andrei/repo/PSGraph/PSGraph.Tests/PSGraph.Tests.csproj`
- `dotnet test /Users/andrei/repo/PSGraph/PSGraph.Tests/PSGraph.Tests.csproj --filter "FullyQualifiedName~VegaDataConverterTests|FullyQualifiedName~ExportGraphViewCmdletTests"`

### Slice VS-08
Status: done
Title: Document PSGraph to PSGraphView migration path
Goal:
- Make the migration understandable to maintainers and future agents
Scope:
- Update docs and explain the target role of PSGraphView
- Document compatibility and remaining slices
Key files:
- `/Users/andrei/repo/PSGraph/README.md`
- `/Users/andrei/repo/PSGraph/.github/prompts/plan-visualSplit.prompt.md`
- `/Users/andrei/repo/PSGraph/.github/prompts/implement-visualSplit.prompt.md`
Definition of done:
- Docs explain that textual exports stay in PSGraph, visual renderers move to PSGraphView, and which slices remain
- Docs also explain that DSM algorithms stay in PSGraph while DSM visualization helpers move to PSGraphView
Implementation notes:
- Updated `/Users/andrei/repo/PSGraph/README.md` to describe the split explicitly: `PSGraph` owns graph/DSM logic plus Graphviz/GraphML/textual export, while visual renderer implementations live in `PSGraphView`.
- Updated `/Users/andrei/repo/PSGraph/docs/Export-Graph.md` and `/Users/andrei/repo/PSGraph/docs/Export-DSM.md` so Vega/MSAGL/DSM matrix rendering is documented as compatibility cmdlet behavior backed by `PSGraphView`.
- Updated `/Users/andrei/repo/PSGraph/docs/Import-Graph.md` to make GraphML ownership explicit as a neutral interchange format that stays in `PSGraph`.
Validation:
- Documentation-only change; no code or test surface changed.

## Execution Order
Recommended order:
1. VS-01
2. VS-02
3. VS-03
4. VS-04
5. VS-05
6. VS-06
7. VS-07
8. VS-08

## Execution Rules For Implementation Agents
- Implement exactly one slice at a time unless the user explicitly requests bundling
- If no slice is specified, pick the first slice with `Status: todo` in execution order
- Update this file after each implementation:
  - change `Status: todo` to `Status: done` or `Status: blocked`
  - append a short `Implementation notes:` paragraph under the slice
  - append `Validation:` with the exact tests or build checks run
- If a requested slice is too large, narrow it to the smallest safe sub-slice and record that in `Implementation notes:`
- Treat PSGraphView as the destination for visualization-specific code even if the current workspace only contains PSGraph

## Validation Strategy
- Keep Export-Graph compatible while moving logic behind service boundaries
- Prefer targeted unit or integration tests over broad, noisy runs
- Verify architecture changes by checking project dependencies as well as behavior
- Do not claim a slice complete without at least one relevant verification step