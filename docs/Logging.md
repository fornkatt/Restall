> **Global logs** are shared across multiple classes and live in a centralized file. **Scoped logs** belongs entirely to a single class and live besides the code. 

Below you can see a comparison table on how we have structured both global and scoped: 

| | Global | Scoped |
|---|---|---|
| **Example** | <img src="../.github/assets/log-global.png"> width="500" | <img src="../.github/assets/log-scoped.png" width="500"> |
| **File** | `Log.<Area>.cs` in `Restall.Application/Logging` | `<Class>.Logging.cs` beside the class |
| **Type** | `public static partial class Log` | `partial class <Class>` |
| **Method** | `public static partial void <Name>(this ILogger logger, …)` | `private partial void Log<Name>(…)` |
| **Prefix** | **NO PREFIX** | `Log` **PREFIX** |
| **Usage** | `_logger.HeroicAppNameNotFound(...)` | `LogIconExtractionFailure(...)` |

### EventIDs and Allocations
Event IDs play a very crucial part of the logging because we can filter our log events, we allocate **25 Event IDs per _global_ file** and **50 per _scoped_ file**, we suggest you to follow this range: 

```
0           reserved — NEVER assign
1 – 499   global logs (Restall.Application.Logging)
500 – 999   reserved for global logs in Infrastructure, if ever split
1000+       scoped logs
```

We highly encourage you to add a header comment to every log that indicates the allocating range:
 
> // Heroic Scanners Logging — EventId range: 75 - 99 

**TIP** 
You can use `grep` to list all the allocated Event Id range across the project and also find the next available Event Id according to the image:

<img src="../.github/assets/log-eventid.png" width="500">

If you want to sort it, make sure to copy the grep example below:
> `grep -rh --include="*.cs" 'EventId range' * | sort -t ':' -k 2 -V` 

---

## Logging Example

This is what the logging looks like:

| Global | Scoped |
|---|---|
| <img src="../.github/assets/log-global-example.png" width="500"> | <img src="../.github/assets/log-scoped-example.png" width="500"> |

**We highly request you follow the following rules when implementing logging in features:**

| Rule | Example |
|---|---|
| **String** parameters get escaped quotes | `\"{GameName}\"` |
| **Numbers, bools, enums** stay bare | `{Count}`, `{Platform}` |
| **Literal identifiers** from external systems get single quotes | `'installed.json'`, `'steamapps'` |
| Hole names are **PascalCase** | `{InstallPath}` — parameter stays `installPath` |
| **No trailing period.** No trailing or double spaces | |
| Em dash joins clauses | `… — all {Platform} Heroic games will be skipped` |
| Provenance marker when relaying a `Result` | `… — Service returned: \"{ErrorMessage}\"` |

---

### Rules when implementing Warning or Error

| Phrase | Means | Captures `Exception`? |
|---|---|---|
| `Failed to …` | An operation was attempted and threw | **Yes** |
| `Could not find …` | A lookup came back empty | **No** |

---

### More rules regarding naming conventions

**Noun first, outcome last.** i.e. `LogUEBinariesCollectionFailure` , we advice you to pick from this list when doing implementation.

`Failure` `NotFound` `Empty` `Start` `Complete` `Success` `Found`

---

## Loglevels and Architecture

**Here you can see log levels we are using and a brief example**

| Log Level | Usage |
|---|---|
| Information | Lifecycle milestones (i.e. `scan started` , `refresh complete`) |
| Debug | Expected absence of per-item detail with no user-visible cost. |
| Warning | A base value was found, but expected derived data was missing/malformed |
| Error | The specific operation the user asked for failed entirely |

---

### Where should you log

For every candidate, ask yourself. Could this fit in `Result.ErrorMessage` + `Exception` ?

> **Yes** → Do not log it, return the `Result`. Logging AND returning creates duplicate failures at two different layers.

> **No** → It is a per-iteration diagnostic detail. Log it locally at `Debug`.

Failures are reported at the layer that knows *what was being attempted*, `Result` carries the cause up to meet it:

```csharp
LogRenoDXVersionReadFailure(addonFilename, request.Game.Name ?? "Unknown",
    renoDxVersion.ErrorMessage, renoDxVersion.Exception);
//  └─ UseCase's context ─┘        └─ service's cause, via Result ─┘
```

---

## PR Checklist

1. Is the log `Global` or `Scoped` ? Is it the right file with the right prefix convention?
2. Is the `Event ID` inside the declared block and **NOT 0** ?
3. Are string quoted  (`\"\"`) ? Are the numbers bare and literals single quoted (`''`) ?
4. Are Hole names `PascalCase` and consistent to the existing methods?
5. Is the name `Noun-first` from the naming conventions?
6. Does the log level pass the healthy-system test?
7. Is Exception always the last parameter and named `ex` ?
8. Does the layer above have better context? If so, return `Result` instead.
9. Does the message contain `Failed to` if it catches an exception? If it doesn't, name it  `Could not find` .
