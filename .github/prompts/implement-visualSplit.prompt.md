---
description: "Use when: implementing a selected visualization split slice"
name: "Implement Visualization Split"
argument-hint: "Укажи phase, first slice или конкретный implementation scope"
agent: "agent"
model: "GPT-5 (copilot)"
---

Реализуй выбранный slice по поддержке или дочистке разделения визуализации между PSGraph и отдельным проектом PSGraphView.

Контекст задачи:
- Архитектурный target для нового visualization проекта: https://github.com/eosfor/PSGraphView.git
- Основная архитектурная линия уже выбрана и реализована: `PSGraph` держит graph/DSM core и textual-interchange export, а `PSGraphView` держит визуальные renderer-ы и visualization cmdlet-ы.
- Внутренняя граница между слоями должна оставаться object-based, а не строиться вокруг serialized export.
- Канонический план и список slice-ов находятся в `/Users/andrei/repo/PSGraph/.github/plans/visualSplit-plan.md`.
- Этот prompt предназначен не для перепланирования всего roadmap, а для выполнения одного конкретного implementation slice за раз без отката к старой архитектуре.

Как работать:
1. Сначала прочитай канонический план из `/Users/andrei/repo/PSGraph/.github/plans/visualSplit-plan.md`.
2. Затем определи, какой именно slice реализации запрошен пользователем:
   - конкретная phase
   - `First Slice`
   - конкретный `VS-0X`
   - `next slice`
   - конкретный файл или subsystem
   - подготовительный refactoring step
3. Если пользователь не указал slice явно, выбери первый `Status: todo` из канонического плана по разделу `Execution Order`.
4. Если пользователь дал слишком широкий scope, сузь задачу до минимального безопасного implementation slice и явно сформулируй, что именно будет сделано сейчас.
5. Исследуй только тот код, который нужен для выбранного slice.
6. Внеси реальные изменения в код, а не только план.
7. Сохрани текущие публичные контракты, если пользователь явно не просил breaking changes.
8. После изменений запусти самые релевантные тесты или другую доступную верификацию.
9. После успешной реализации обнови `/Users/andrei/repo/PSGraph/.github/plans/visualSplit-plan.md`:
   - поменяй статус slice
   - добавь короткие implementation notes
   - зафиксируй validation
10. В финале кратко опиши, что реализовано, что проверено, и что остается следующим шагом.

Ключевые ограничения:
- Не перепроектируй весь модуль целиком, если запрошен только один slice.
- Не смешивай задачу выноса visualization с redesign графовых алгоритмов или DSM.
- Не возвращай visual format-ы или renderer-specific зависимости обратно в `PSGraph`.
- Не делай большой перенос кода в один заход, если можно сделать boundary step локально.
- Если target repo `PSGraphView` недоступен в текущем workspace, делай preparatory changes в текущем репозитории так, чтобы следующий шаг переноса в `PSGraphView` был очевиден.
- Любые инструкции по переносу должны считать `PSGraphView` конечным target для visualization-specific code.
- Не придумывай новые slice-ы, если задачу можно сопоставить существующему `VS-0X`; если нужен под-slice, привяжи его к существующему slice и зафиксируй это в plan file.

Предпочтительный тип задач для этого prompt:
- выделить adapter/service boundary
- подготовить contracts для выноса visualization
- очистить core-модели от rendering-specific деталей
- дочистить docs, tests или shared contracts после уже выполненного split
- убрать устаревшие compatibility references
- добавить или обновить тесты для подтверждения текущих границ между `PSGraph` и `PSGraphView`

Опорные файлы:
- [plan-visualSplit.prompt.md](/Users/andrei/repo/PSGraph/.github/prompts/plan-visualSplit.prompt.md)
- [visualSplit-plan.md](/Users/andrei/repo/PSGraph/.github/plans/visualSplit-plan.md)
- [ExportGraphViewCmdLet.cs](/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.cs)
- [ExportGraphViewCmdLet.TreeLayout.cs](/Users/andrei/repo/PSGraph/PSGraph/cmdlets/Graph/ExportGraphViewCmdLet.TreeLayout.cs)
- [VegaForceDirectedExporter.cs](/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/VegaForceDirectedExporter.cs)
- [GraphViewVegaExtensions.cs](/Users/andrei/repo/PSGraphView/src/PSGraphView.Vega/GraphViewVegaExtensions.cs)
- [PSGraph.csproj](/Users/andrei/repo/PSGraph/PSGraph/PSGraph.csproj)
- [PSGraph.Common.csproj](/Users/andrei/repo/PSGraph/PSGraph.Common/PSGraph.Common.csproj)
- [PSVertex.cs](/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSVertex.cs)
- [PSEdge.cs](/Users/andrei/repo/PSGraph/PSGraph.Common/Model/PSEdge.cs)
- [GraphExportTypes.cs](/Users/andrei/repo/PSGraph/PSGraph.Common/Model/GraphExportTypes.cs)
- [ExportGraphViewCmdletTests.cs](/Users/andrei/repo/PSGraph/PSGraph.Tests/ExportGraphViewCmdletTests.cs)
- [ExportDSMCmdlet.cs](/Users/andrei/repo/PSGraph/PSGraph/cmdlets/DSM/ExportDSMCmdlet.cs)

Решения, которые считать дефолтными, если пользователь не указал иное:
- Основной контракт между слоями: object-based
- GraphML: interchange-формат, не внутренний обязательный transport
- Первый приоритет: минимальные обратимо-совместимые изменения
- Не возвращай compatibility façade в `PSGraph`, если пользователь явно не просил временный bridge step

Ожидаемое поведение агента:
- Самостоятельно выбрать минимальный безопасный шаг реализации
- Использовать канонический plan file как source of truth по slice-ам и их порядку
- Выполнить кодовые изменения end-to-end
- Проверить результат тестами, сборкой или точечной верификацией
- Не останавливаться на описании решения, если пользователь просит implementation

Ожидаемый формат ответа:

## Scope
Коротко сформулируй, какой именно implementation slice выполняется сейчас.

## Changes
Опиши, какие изменения внесены и в каких частях системы.

## Compatibility
Отметь, как сохранена совместимость и есть ли осознанные отклонения.

## Validation
Перечисли, какие проверки, тесты или сборки были запущены.

## Next Step
Назови следующий логичный slice после текущего.

Требования к качеству результата:
- Делай реальные изменения в коде, если пользователь просит implementation.
- Держи diff минимальным и локальным.
- Подтверждай изменения релевантной верификацией.
- Привязывай решения к конкретным файлам и типам.
- Если чего-то нельзя сделать в текущем workspace, подготовь ближайший безопасный шаг и явно укажи, что должно уйти в `PSGraphView` на следующем этапе.