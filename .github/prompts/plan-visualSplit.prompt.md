---
description: "Use when: splitting visualization from graph core"
name: "Plan Visualization Split"
argument-hint: "Опиши репозиторий и задачу по выносу visualization layer"
agent: "plan"
model: "GPT-5 (copilot)"
---

Помоги спроектировать вынос визуализации из текущего репозитория в отдельный проект.

Контекст задачи:
- Сейчас графовая логика, экспорт и визуализация слишком тесно связаны.
- Нужно понять, как отделить visualization layer от core без лишних breaking changes.
- Основной вопрос: что должно быть границей между модулями — графовый объект в памяти или сериализованный экспорт.

Цель:
Подготовь архитектурное решение и пошаговый план миграции для выноса визуализации в отдельный проект, сохранив совместимость с текущим пользовательским API настолько, насколько это разумно.

Что нужно сделать:
1. Изучи, как сейчас связаны core, export и visualization.
2. Определи, какие части должны остаться в core, а какие стоит вынести в отдельный visualization project.
3. Прими явное решение по контракту между слоями:
   object-based API или export-based API.
4. Предложи минимально болезненный путь миграции без резкого переписывания публичных cmdlet-ов.
5. Отдельно опиши, что делать с GraphML: считать его interchange-форматом или частью visualization/export слоя.
6. Укажи, какие зависимости и типы сегодня создают лишнюю сцепку и должны быть вынесены или переработаны.
7. Составь фазы реализации так, чтобы ими можно было пользоваться как backlog для внедрения.
8. Инструкции по реализации должны указывать что в качестве target для нового проекта должен быть использован https://github.com/eosfor/PSGraphView.git

Предпочтительное архитектурное направление:
- Не использовать сериализованный экспорт как основной внутренний контракт между core и visualization.
- Считать основной рекомендацией object-based границу: либо передача `PsBidirectionalGraph`, либо нейтрального `GraphView` DTO.
- Экспорт в `GraphML`, `Vega JSON`, `SVG`, `DOT` оставить внешним API для обмена, публикации и сохранения результатов.
- Миграцию строить эволюционно: сначала adapters и contracts, потом перенос реализаций, потом возможное разделение на отдельный PowerShell-модуль.

На что обратить особое внимание:
- Не сломать `Export-Graph` без веской причины.
- Не смешивать redesign алгоритмов графов и DSM с задачей выноса визуализации.
- Зафиксировать, какие зависимости должны уйти из core-проектов.
- Проверить, не протекают ли Graphviz/MSAGL/Vega-детали в общие модели.

Опорные файлы для анализа:
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
- [VegaDataConverterTests.cs](/Users/andrei/repo/PSGraph/PSGraph.Tests/VegaDataConverterTests.cs)
- [README.md](/Users/andrei/repo/PSGraph/README.md)

Ожидаемый формат ответа:

## Recommendation
Сформулируй одно явное архитектурное решение и коротко объясни, почему оно лучше альтернатив.

## Boundary
Опиши рекомендуемую границу между core и visualization.
Явно ответь:
- что остается в core
- что уходит в visualization project
- где должен жить GraphML

## Coupling Today
Перечисли ключевые точки сцепления в текущем коде.

## Migration Plan
Дай пошаговый план по фазам.
Для каждой фазы укажи:
- цель
- ключевые изменения
- риски
- критерий завершения

## Compatibility
Опиши, как сохранить совместимость для текущего `Export-Graph` и связанных сценариев.

## First Slice
Предложи самый безопасный первый implementation slice, который можно реально начать делать сразу.

## Validation
Опиши, какими тестами и проверками подтвердить, что декуплинг сделан правильно.

Требования к качеству ответа:
- Не ограничивайся общими словами, привязывай выводы к конкретным файлам и типам.
- Не предлагай большой рефакторинг без поэтапной миграции.
- Если есть несколько вариантов, выбери один как основной, а остальные кратко сравни как альтернативы.
- Ответ должен быть пригоден как основа для технического RFC или implementation plan.