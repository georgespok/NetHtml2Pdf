# Contracts: Formatting Context Behaviour

## Purpose
Formalise the observable contracts for the new formatting contexts. Tests and documentation MUST reference these behaviours when feature flags are enabled.

## InlineBlockFormattingContext
- **Contract**: Given a `display:inline-block` element with intrinsic width/height, the context emits a fragment with matching dimensions and baseline aligned to surrounding inline flow.
- **Flag**: `EnableInlineBlockContext` MUST be true.
- **Diagnostics**: When downgraded (flag off), log `FormattingContext.Downgrade` with node path and continue legacy flow.
- **Tests**: Behaviour tests assert fragment width/height, pagination carry-over, and baseline alignment logs.

## TableFormattingContext
- **Contract**: With table header/body/footer sections, pagination MUST repeat header rows on new pages and maintain column widths within +/-1pt, provided `EnableTableContext` is true.
- **Border Handling**: While `EnableTableContext` is true and `EnableTableBorderCollapse` is false, treat `border-collapse: collapse` as `separate` and log a downgrade warning.
- **Diagnostics**: Emit `TableContext.HeaderRepeated` and `TableContext.BorderDowngrade` structured logs when applicable.
- **Tests**: Multi-page fixtures verify header repetition, footer placement, and warning logs.

## FlexFormattingContext (Preview)
- **Contract**: For `display:flex` containers with `flex-direction: row|column`, items are positioned according to justify/align settings while `flex-wrap` is forced to `nowrap`.
- **Unsupported Properties**: Encountering unsupported properties (e.g., `flex-basis:auto`, `flex-wrap:wrap`) MUST trigger a downgrade warning and fallback to legacy handling.
- **Diagnostics**: Emit `FlexContext.Downgrade` with reason code. No structural exceptions thrown.
- **Tests**: Rendering fixtures verify item order, spacing, and that downgrades produce warnings.

## Pagination & Adapter Integration
- All contexts MUST output fragments compatible with `PaginationService.Paginate` without requiring adapter awareness of specific context types.
- Adapters MUST continue to consume fragments via `IRendererAdapter` without referencing context-specific classes; validation via unit tests ensures adapter usage remains generic.

## Performance Contract
- Enabling each flag individually or combined MUST keep render time within +5% of legacy baseline measured on the 50-page fixture.
- Performance tests record before/after metrics and are stored alongside regression data.

## Compliance Checklist
- [ ] Feature flag default remains `false`.
- [ ] Structured logging verified for downgrade scenarios.
- [ ] Pagination outputs validated (headers, carry-over, alignment).
- [ ] Reflection-free tests covering observable behaviour.


