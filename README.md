9# Библиотека: каталог, выдачи и отчёты

Курсовая работа по дисциплине «Программирование на C#»
Учёт книжного фонда и читателей, оформление выдач и возвратов, расчёт просрочек и штрафов,
аналитические отчёты, обмен данными через CSV и JSON.

## Технологии

- .NET 9 (`net9.0-windows`), WPF, `UseWPF=true`, `OutputType=WinExe`
- MVVM реализован вручную: `RelayCommand`, `AsyncRelayCommand`, `ViewModelBase`,
  `ValidatableViewModelBase` (без CommunityToolkit.Mvvm, Prism, Caliburn.Micro, ReactiveUI, MVVM Light)
- Хранение — JSON через `System.Text.Json`; базы данных не используются
- Сторонние NuGet-пакеты отсутствуют
