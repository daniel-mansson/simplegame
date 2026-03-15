---
estimated_steps: 6
estimated_files: 6
---

# T02: Define MVP base types, sample view interface, and domain service

**Slice:** S01 — Core MVP Infrastructure & Project Setup
**Milestone:** M001

## Description

Define the core MVP type system that all downstream slices depend on: `IView` marker interface, `Presenter<TView>` generic base class, `UIFactory` central construction point, a sample view interface (`ISampleView`), a sample domain service (`GameService`), and a concrete sample presenter (`SamplePresenter`) that wires them together. These types establish the conventions for view independence, constructor injection, and presenter lifecycle that S02–S05 will follow.

## Steps

1. Create `Assets/Scripts/Core/MVP/IView.cs` — empty marker interface in `SimpleGame.Core.MVP` namespace:
   ```csharp
   namespace SimpleGame.Core.MVP
   {
       public interface IView { }
   }
   ```

2. Create `Assets/Scripts/Core/MVP/Presenter.cs` — generic base class:
   ```csharp
   namespace SimpleGame.Core.MVP
   {
       public abstract class Presenter<TView> where TView : IView
       {
           protected readonly TView View;

           protected Presenter(TView view)
           {
               View = view;
           }

           public virtual void Initialize() { }
           public virtual void Dispose() { }
       }
   }
   ```
   Key design notes: `View` is `protected readonly` — subclasses access it, but it's set once. `Initialize()` is separate from constructor for two-phase init (needed when async setup comes in S02). `Dispose()` is virtual for cleanup (forward intelligence from research).

3. Create `Assets/Scripts/Core/MVP/ISampleView.cs` — sample view interface demonstrating the convention:
   ```csharp
   using System;

   namespace SimpleGame.Core.MVP
   {
       public interface ISampleView : IView
       {
           event Action OnButtonClicked;
           void UpdateLabel(string text);
       }
   }
   ```
   Uses `event Action` (not Unity Button.onClick) — this is the convention for all view interfaces.

4. Create `Assets/Scripts/Core/Services/GameService.cs` — plain C# domain service:
   ```csharp
   namespace SimpleGame.Core.Services
   {
       public class GameService
       {
           public string GetWelcomeMessage()
           {
               return "Welcome to Simple Game";
           }
       }
   }
   ```
   Deliberately simple — proves the injection pattern without business logic complexity.

5. Create `Assets/Scripts/Core/MVP/SamplePresenter.cs` — concrete presenter:
   ```csharp
   using SimpleGame.Core.Services;

   namespace SimpleGame.Core.MVP
   {
       public class SamplePresenter : Presenter<ISampleView>
       {
           private readonly GameService _gameService;

           public SamplePresenter(ISampleView view, GameService gameService) : base(view)
           {
               _gameService = gameService;
           }

           public override void Initialize()
           {
               View.OnButtonClicked += HandleButtonClicked;
               View.UpdateLabel(_gameService.GetWelcomeMessage());
           }

           public override void Dispose()
           {
               View.OnButtonClicked -= HandleButtonClicked;
           }

           private void HandleButtonClicked()
           {
               View.UpdateLabel(_gameService.GetWelcomeMessage());
           }
       }
   }
   ```
   Demonstrates: constructor injection, event subscription in Initialize, cleanup in Dispose, service usage.

6. Create `Assets/Scripts/Core/MVP/UIFactory.cs` — central factory:
   ```csharp
   using SimpleGame.Core.Services;

   namespace SimpleGame.Core.MVP
   {
       public class UIFactory
       {
           private readonly GameService _gameService;

           public UIFactory(GameService gameService)
           {
               _gameService = gameService;
           }

           public SamplePresenter CreateSamplePresenter(ISampleView view)
           {
               return new SamplePresenter(view, _gameService);
           }
       }
   }
   ```
   Factory receives services once at construction, passes them per-presenter. Each `Create` method takes the view interface and returns the wired presenter.

## Must-Haves

- [ ] `IView` is an empty marker interface — no methods, no properties
- [ ] `Presenter<TView>` constrains `TView : IView`, stores view as `protected readonly`, has virtual `Initialize()` and `Dispose()`
- [ ] `ISampleView` extends `IView` with `event Action OnButtonClicked` and `void UpdateLabel(string text)` — no Unity types in the interface
- [ ] `GameService` is a plain C# class with no MonoBehaviour, no static state
- [ ] `SamplePresenter` receives view and service via constructor, subscribes to events in Initialize, unsubscribes in Dispose
- [ ] `UIFactory` receives `GameService` in constructor, has `CreateSamplePresenter(ISampleView)` method
- [ ] No `static` fields holding state in any file
- [ ] No `using UnityEngine` in any core type (except if needed for Debug.Log — avoid even that)
- [ ] All files in `SimpleGame.Core.MVP` or `SimpleGame.Core.Services` namespace

## Verification

- Project compiles in Unity batchmode with zero errors
- `grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing
- No `MonoBehaviour` inheritance in any presenter or service class
- View interface has no backward references (no presenter/service/model types in ISampleView)

## Observability Impact

- **New compilation surface:** Adding 6 `.cs` files to `Assets/Scripts/Core/` expands the `SimpleGame.Runtime` assembly. Compilation errors will appear in `Logs/Editor.log` as `error CS` lines referencing these files by path.
- **Namespace resolution signal:** If `SimpleGame.Core.MVP` or `SimpleGame.Core.Services` can't be resolved from the test assembly, Unity logs `Assembly 'SimpleGame.Tests.EditMode' will not be loaded due to errors` — indicates the test assembly reference chain is broken.
- **Static state check (diagnostic command):** `grep -r "static " --include="*.cs" Assets/Scripts/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` — should return nothing; any hit indicates an unexpected static field.
- **MonoBehaviour contamination check:** `grep -r "MonoBehaviour" --include="*.cs" Assets/Scripts/Core/` — must return nothing for all core types.
- **View interface isolation check:** `grep -rn "Presenter\|GameService" Assets/Scripts/Core/MVP/ISampleView.cs` — must return nothing; view must have no backward references.
- **Failure artifact:** If batchmode exits non-zero after adding these files, `Logs/Editor.log` is the primary failure surface — search for `error CS` near the new file paths.

## Inputs

- `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime assembly definition from T01
- Compiled Unity project with UniTask available from T01
- Folder structure: `Assets/Scripts/Core/MVP/`, `Assets/Scripts/Core/Services/` from T01

## Expected Output

- `Assets/Scripts/Core/MVP/IView.cs` — marker interface
- `Assets/Scripts/Core/MVP/Presenter.cs` — generic base class
- `Assets/Scripts/Core/MVP/ISampleView.cs` — sample view interface
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — concrete presenter
- `Assets/Scripts/Core/MVP/UIFactory.cs` — central factory
- `Assets/Scripts/Core/Services/GameService.cs` — domain service
