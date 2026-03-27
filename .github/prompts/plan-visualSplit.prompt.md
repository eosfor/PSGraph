---
description: "Use when: planning follow-up work for the PSGraph and PSGraphView split"
name: "Plan Visualization Split"
argument-hint: "Опиши репозиторий и задачу по выносу visualization layer"
agent: "plan"
model: "GPT-5 (copilot)"
---

Помоги спроектировать следующий этап или cleanup после выноса визуализации из текущего репозитория в отдельный проект.

Контекст задачи:
- Основное разделение уже выполнено: `PSGraph` держит graph/DSM core и textual-interchange export, `PSGraphView` держит visual renderer-ы и visualization cmdlet-ы.
- Нужно планировать только те шаги, которые усиливают или дочищают эту границу без лишних breaking changes.
- Основной вопрос уже решен: граница между модулями должна оставаться object-based, а не строиться вокруг serialized export.

Цель:
Подготовь архитектурное решение и пошаговый план для следующего этапа после разделения визуализации, сохранив текущие границы между `PSGraph` и `PSGraphView`.

Что нужно сделать:
1. Изучи, как сейчас связаны core, export и visualization.
2. Определи, какие части должны остаться в core, а какие стоит вынести в отдельный visualization project.
3. Исходи из уже принятого object-based контракта между слоями и не переоткрывай этот выбор без явного запроса.
4. Предложи минимально болезненный путь следующих изменений без резкого переписывания публичных cmdlet-ов.
5. Отдельно проверь, не размывается ли текущая роль GraphML как interchange-формата в `PSGraph`.
6. Укажи, какие зависимости и типы сегодня создают лишнюю сцепку и должны быть вынесены или переработаны.
7. Составь фазы реализации так, чтобы ими можно было пользоваться как backlog для внедрения.
8. Инструкции по реализации должны указывать что в качестве target для нового проекта должен быть использован https://github.com/eosfor/PSGraphView.git

Предпочтительное архитектурное направление:
- Не использовать сериализованный экспорт как основной внутренний контракт между core и visualization.
- Считать основной рекомендацией object-based границу: либо передача `PsBidirectionalGraph`, либо нейтрального `GraphView` DTO.
- `GraphML` и `DOT` считать внешним API `PSGraph`, а визуальные форматы и view-export держать в `PSGraphView`.
- Любой follow-up план должен сохранять уже достигнутое разделение, а не возвращать compatibility path-ы в `PSGraph`.

На что обратить особое внимание:
- Не сломать `Export-Graph` без веской причины.
- Не смешивать redesign алгоритмов графов и DSM с задачей выноса визуализации.
- Зафиксировать, какие зависимости должны уйти из core-проектов.
- Проверить, не протекают ли Graphviz/MSAGL/Vega-детали в общие модели.
- Не предлагать возврат visual format-ов в enum-ы или cmdlet-ы `PSGraph`, если на это нет явного запроса.

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
- [ExportDSMCmdlet.cs](/Users/andrei/repo/PSGraph/PSGraph/cmdlets/DSM/ExportDSMCmdlet.cs)
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