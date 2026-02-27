# Lightweight Unity DI Container

A lightweight, performant Dependency Injection (DI) container for Unity, designed to mimic the API of [Zenject (Extenject)](https://github.com/mathieubour/Zenject).

## 🎯 Project Goal & Purpose

The primary goal of this project was to create a flexible, low-overhead DI solution for Unity projects where a full-scale framework like Zenject would be overkill. It serves two main purposes:
1.  **Optimization:** Providing a minimal runtime footprint for mobile or web-based Unity games that require modular architecture without the bloat.
2.  **Portfolio Demonstration:** Showcasing core architecture skills, understanding of Inversion of Control (IoC) containers, reflection, memory management, and Unity Editor tool development.

## 🔑 Key Features

*   **Zenject-like Fluent API:** Familiar syntax (`Bind<T>`, `FromInstance`, `AsSingle`, etc.) for easy adoption.
*   **Multiple Injection Strategies:**
    *   **Constructor Injection:** Automatic resolution for plain C# classes (POCOs).
    *   **Field & Method Injection:** Supports `[Inject]` attribute for `MonoBehaviours`.
*   **Conditional Binding:** Supports `WhenInjected<T>` to specify different implementations for different consumers.
*   **Hierarchical Contexts:** Supports context stacking to manage dependencies across different game states or modules.
*   **Circular Dependency Detection:** Built-in depth-first search algorithm to detect and prevent infinite recursion during object graph resolution.
*   **Integrated Object Pooling:** A robust pooling system (`IPoolManager`) integrated directly into the binding syntax.
*   **Editor Debugging Tools:** Custom Unity Editor windows to visualize the dependency graph, active contexts, and pool status.

## 🛠 Technical Implementation & Techniques

### 1. Dependency Resolution & Cycle Detection
The core utilizes a recursive `DependencyBuilder` to construct the object graph. It employs a depth-tracking mechanism to identify circular dependencies (e.g., Class A needs Class B, which needs Class A) before instantiation, throwing a descriptive error rather than causing a stack overflow.

### 2. Reflection Strategy
To avoid the high cost of repeated reflection calls, the `DefaultInjectionStrategy` and `DependencyResolver` use dictionary-based caching:
*   **POCOs:** Automatically identifies the first constructor with parameters.
*   **Unity Objects:** Scans for methods and fields marked with the `[Inject]` attribute.

### 3. Factory & Pooling Pattern
The container includes a flexible Factory system. The `BindingConstructor` allows switching between standard instantiation, prefab instantiation, and pooled instantiation via a fluent interface:

```csharp
// Example: Bind a factory that automatically creates an expanding pool
context.BindIFactory<BaseControl<IComponentModel>>()
    .FromPrefab(_prefabPath.Milestone)
    .WithPool(8); 
```

## 🐞 Editor Debugging Tools

The project includes custom Unity Editor windows to help visualize the runtime state.

### DI Debug Manager (`DI/Debug manager`)
Allows you to inspect the current state of the dependency graph.
*   **Context Stack:** View the hierarchy of active contexts.
*   **Binding Inspector:** See exactly what is bound in each context.
*   **Transient Analysis:** Track which objects depend on transient bindings.
*   **Disposable Check:** Quickly identify objects that implement `IDisposable`.

### Pool Manager (`DI/Pool manager`)
Provides real-time monitoring of the object pooling system.
*   View all active pools by prefab name.
*   Monitor current element counts in every pool.

## 🚀 Usage Examples

### 1. Installer Setup
Inherit from `Installer` to define your dependency graph.

```csharp
public class GameInstaller : Installer
{
    [SerializeField] private PlayerConfig _playerConfig;

    public override void Install(IBinder context)
    {
        // Bind an interface to a concrete implementation
        context.BindInterfaces<InputService>();
        
        // Bind a specific instance
        context.BindInstance(_playerConfig).WithInterfaces();
    }
}
```

### 2. Conditional Binding
You can inject different implementations based on the class asking for the dependency.

```csharp
// Inject 'GoldIcon' when 'ShopView' asks for IIcon
context.Bind<IIcon>().FromInstance(new GoldIcon()).WhenInjected<ShopView>();

// Inject 'GemIcon' when 'InventoryView' asks for IIcon
context.Bind<IIcon>().FromInstance(new GemIcon()).WhenInjected<InventoryView>();
```

### 3. Constructor Injection (Pure C# Classes)
For non-MonoBehaviour classes, dependencies are injected automatically via the constructor.

```csharp
public class InputService : IInputService
{
    private readonly PlayerConfig _config;

    // No [Inject] attribute needed here
    public InputService(PlayerConfig config)
    {
        _config = config;
    }
}
```

### 4. Field & Method Injection (MonoBehaviours)
For Unity components, use the `[Inject]` attribute.

```csharp
public class PlayerController : MonoBehaviour
{
    private IInputService _input;

    // Field Injection
    [Inject] private ILogger _logger;

    // Method Injection
    [Inject]
    public void Construct(IInputService input)
    {
        _input = input;
    }
}
```

## ✅ Problems Solved

*   **Decoupling:** Removes hard dependencies between classes, making the code testable and modular.
*   **Boilerplate Reduction:** Automates the wiring of complex object graphs.
*   **Memory Fragmentation:** The integrated `WithPool` binding drastically reduces garbage collection spikes by reusing objects transparently.
*   **Lifecycle Management:** Automates initialization and disposal of services based on the context lifecycle.
*   **Visibility:** Custom editor tools solve the "black box" problem of DI containers by visualizing the hidden graph.

## 📝 TODO / Known Limitations

While fully functional for the intended scope, the following features are planned or currently missing compared to full-featured frameworks:

*   **Signals System:** No built-in event bus/signal system (unlike Zenject Signals).
*   **Async Injection:** No native support for `Task` or `UniTask` based resolution.
*   **Scene Context Automation:** Currently requires manual setup via `Root` and `ContextManager` rather than dragging a "SceneContext" prefab into the scene.
