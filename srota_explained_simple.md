# Srota.Core - Explained 

## 🎯 What is Srota?

Imagine you have a **robot helper** that can do boring jobs for you over and over again, like:
- 🔔 Check your mailbox every 5 minutes
- 📨 Listen for text messages and reply
- 🧹 Clean your room every day at 3pm

**Srota is that robot!** It's a computer program that does tasks automatically in the background.

---

## 📁 The Project Structure (Like Your School Backpack)

```
Srota.Core/
├── Abstractions/     👈 "The Rules" - What the robot MUST know how to do
├── Builders/         👈 "The Instructions" - How you tell the robot what to do
├── Core/             👈 "The Brain" - The actual robot that does the work
├── Tasks/            👈 "The Actions" - Different jobs the robot can do
└── Models/           👈 "The Messages" - What information looks like
```

---

## 1️⃣ ABSTRACTIONS FOLDER - "The Rules Book" 📖

### ISrotaWorker.cs - "What Every Robot Must Know"

```csharp
public interface ISrotaWorker : IDisposable
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    bool IsRunning { get; }
}
```


Think of this like a **instruction manual for all robots**. Every robot MUST know how to:

1. **StartAsync** - "Wake up and start working!"
   - Like pressing the "ON" button on your toy robot

2. **StopAsync** - "Go to sleep and stop working!"
   - Like pressing the "OFF" button

3. **IsRunning** - "Are you awake right now?"
   - Like checking if your robot's eyes are glowing (yes/no)

4. **IDisposable** - "Clean up your mess when you're done!"
   - Like putting your toys away when finished playing

---

### IEventSource.cs - "Where Messages Come From"

```csharp
public interface IEventSource<T> : IDisposable
{
    Task<T> ReadAsync(CancellationToken cancellationToken);
}
```


Imagine a **mailbox** where messages arrive:

```csharp
IEventSource<T>
```
- `T` is the **type of message** (like a letter, package, or postcard)
- This is like saying: "I have a mailbox, and letters come out of it"

```csharp
Task<T> ReadAsync(CancellationToken cancellationToken);
```
- **ReadAsync** = "Wait for a message and give it to me"
- Like standing by your mailbox waiting for the mail truck
- When a letter arrives, you grab it!

---

### ITaskDefinition.cs - "A Job Description"

```csharp
internal interface ITaskDefinition
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
```



Every **job** (chore) has:

1. **Name** - What do we call this job?
   - "Take out trash"
   - "Feed the cat"

2. **ExecuteAsync** - Actually DO the job!
   - This is like when you actually feed the cat (not just talk about it)

---

## 2️⃣ MODELS FOLDER - "What Things Look Like" 📦

### SseEvent.cs - "A Special Message"

```csharp
public class SseEvent
{
    public string? EventType { get; set; }
    public string? Data { get; set; }
    public string? Id { get; set; }
    public int? Retry { get; set; }
}
```



This is like receiving a **special package** with a label:

- **EventType** = What kind of message is this? ("birthday", "alert", "news")
- **Data** = The actual message inside ("Happy Birthday!" or "Dinner is ready!")
- **Id** = A tracking number (so you know which message it is)
- **Retry** = If the message doesn't arrive, try again in X seconds

Example:
```
📬 Package arrives:
   Type: "birthday"
   Data: "Happy Birthday Sarah!"
   Id: "msg_12345"
   Retry: 30 (try again in 30 seconds if failed)
```

---

## 3️⃣ BUILDERS FOLDER - "How to Give Instructions" 🏗️

### WorkerBuilder.cs - "The Main Instruction Sheet"

```csharp
public class WorkerBuilder
{
    private readonly List<ITaskDefinition> _tasks = new();
    private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
    private Action<Exception, string>? _globalErrorHandler;
```



Imagine you have a **notebook** where you write down all the chores:

```csharp
private readonly List<ITaskDefinition> _tasks = new();
```
- This is your **TO-DO LIST** 📝
- `List<ITaskDefinition>` means "a list of jobs to do"

```csharp
private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
```
- This is like a **diary** 📔
- The robot writes down what it's doing: "3pm - Fed the cat ✓"

```csharp
private Action<Exception, string>? _globalErrorHandler;
```
- This is your **emergency contact** 📞
- If something goes wrong, who do we call?

---

```csharp
public static WorkerBuilder Create() => new();
```


- "Get me a fresh, new notebook to write my to-do list!"

---

```csharp
public WorkerBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
{
    _loggerFactory = loggerFactory;
    return this;
}
```


- "Here's the diary I want to use to write down what happens"
- `return this` means: "Give me back the notebook so I can write more"

---

```csharp
public WorkerBuilder OnError(Action<Exception, string> handler)
{
    _globalErrorHandler = handler;
    return this;
}
```


- "If anything goes wrong, call THIS person!"
- Like giving your robot your mom's phone number in case of emergency

---

```csharp
public PollingTaskBuilder AddPolling(string name, TimeSpan every)
{
    return new PollingTaskBuilder(this, name, every);
}
```



**"Add a REPEATING job to my list!"**

- `name` = What should we call this job? ("Check mailbox")
- `every` = How often should we do it? (Every 5 minutes)

Example:
```csharp
.AddPolling("Check mailbox", TimeSpan.FromMinutes(5))
```
= "Check the mailbox every 5 minutes"

---

```csharp
public EventTaskBuilder<T> AddEvent<T>(string name, Func<IEventSource<T>> sourceFactory)
{
    return new EventTaskBuilder<T>(this, name, sourceFactory);
}
```



**"Listen for messages and do something when they arrive!"**

- Like waiting for text messages on your phone
- When a text comes in → read it and reply!

Example:
```csharp
.AddEvent("Text messages", () => myPhoneMailbox)
    .Do(message => ReplyToMessage(message))
```

---

```csharp
public ISrotaWorker Build()
{
    return new SrotaWorker(_tasks, _loggerFactory, _globalErrorHandler);
}
```



**"Take my to-do list and CREATE the robot!"**

- You've written down all the chores
- Now you build the robot that will do them!
- Like giving instructions to a toy robot and pressing "START"

---

### PollingTaskBuilder.cs - "Repeating Job Instructions"

```csharp
public class PollingTaskBuilder
{
    private readonly WorkerBuilder _parent;
    private readonly string _name;
    private readonly TimeSpan _interval;
    private int _maxRetries = 3;
    private TimeSpan _retryDelay = TimeSpan.FromSeconds(5);
```



This is like a **specific chore card**:

```csharp
private readonly string _name;
```
- **Name of chore**: "Take out trash"

```csharp
private readonly TimeSpan _interval;
```
- **How often**: Every day at 3pm

```csharp
private int _maxRetries = 3;
```
- **If you fail, try again**: If you drop the trash bag, pick it up and try 3 more times

```csharp
private TimeSpan _retryDelay = TimeSpan.FromSeconds(5);
```
- **Wait between retries**: Wait 5 seconds before trying again

---

```csharp
public PollingTaskBuilder WithRetry(int maxRetries, TimeSpan retryDelay)
{
    _maxRetries = maxRetries;
    _retryDelay = retryDelay;
    return this;
}
```



"If I mess up, here's what to do:"
- Try again **X times** (`maxRetries`)
- Wait **Y seconds** between tries (`retryDelay`)

Example:
```csharp
.WithRetry(maxRetries: 5, retryDelay: TimeSpan.FromSeconds(10))
```
= "Try 5 times, waiting 10 seconds between each try"

---

```csharp
public WorkerBuilder Do(Func<Task> handler)
{
    _handler = handler;
    _parent.RegisterTask(new PollingTaskDefinition(...));
    return _parent;
}
```



**"Here's WHAT to do for this chore!"**

- `handler` = The actual work (like "take bag to curb")
- `RegisterTask` = Write it on the to-do list
- `return _parent` = Give back the main notebook

Example:
```csharp
.Do(async () => await TakeOutTrash())
```
= "The job is: take out the trash"

---

## 4️⃣ TASKS FOLDER - "The Actual Jobs" 💼

### PollingTaskDefinition.cs - "A Repeating Job"

```csharp
internal class PollingTaskDefinition : ITaskDefinition
{
    private readonly TimeSpan _interval;
    private readonly Func<Task> _handler;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;
    
    public string Name { get; }
```



This is the **actual chore card** with all details:

- **Name**: "Water the plants"
- **_interval**: Do it every 2 days
- **_handler**: The actual work (grab watering can, pour water)
- **_maxRetries**: Try 3 times if you spill
- **_retryDelay**: Wait 1 minute between tries

---

```csharp
public async Task ExecuteAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        // ... do the work repeatedly
        await Task.Delay(_interval, cancellationToken);
    }
}
```



```csharp
while (!cancellationToken.IsCancellationRequested)
```
**"Keep doing this job until someone tells me to stop!"**

- Like a video game that keeps running until you press PAUSE
- `cancellationToken` = The STOP button

```csharp
await Task.Delay(_interval, cancellationToken);
```
**"Wait before doing it again"**

- After watering plants, wait 2 days before watering again
- Like a timer on your phone

---

**The Retry Logic:**

```csharp
var retries = 0;
while (retries <= _maxRetries)
{
    try
    {
        await _handler(); // Do the job
        break; // Success! Stop trying
    }
    catch (Exception ex) when (retries < _maxRetries)
    {
        retries++; // Failed! Count it
        await Task.Delay(_retryDelay, cancellationToken); // Wait a bit
    }
    catch (Exception ex)
    {
        throw; // All tries failed, give up!
    }
}
```



**Imagine trying to throw a ball into a basket:**

1. **Try #1**: Throw ball → Miss! 😢
2. **Wait 5 seconds** (retryDelay)
3. **Try #2**: Throw ball → Miss again! 😢
4. **Wait 5 seconds**
5. **Try #3**: Throw ball → Success! 🎉
6. **Stop trying** (break)

If all 3 tries fail:
- **Give up** and tell someone "I couldn't do it!" (throw)

---

### EventTaskDefinition.cs - "Listening for Messages"

```csharp
public async Task ExecuteAsync(CancellationToken cancellationToken)
{
    using var source = _sourceFactory();
    while (!cancellationToken.IsCancellationRequested)
    {
        var item = await source.ReadAsync(cancellationToken);
        await _handler(item);
    }
}
```



**Like waiting for text messages:**

```csharp
using var source = _sourceFactory();
```
- **Open your phone** (get the message source)

```csharp
while (!cancellationToken.IsCancellationRequested)
```
- **Keep checking** until bedtime

```csharp
var item = await source.ReadAsync(cancellationToken);
```
- **Wait for a new text message** 📱
- When one arrives, grab it!

```csharp
await _handler(item);
```
- **Read and reply** to the message

Example in real life:
```
🔔 *Ding!* New message: "Want to play?"
→ Read it
→ Reply: "Sure! Be there in 5 min"
→ Wait for next message
🔔 *Ding!* New message: "Bring your soccer ball"
→ Read it
→ Reply: "Got it!"
→ Wait for next message...
```

---

## 5️⃣ CORE FOLDER - "The Robot's Brain" 🤖

### SrotaWorker.cs - "The Actual Robot"

```csharp
internal class SrotaWorker : ISrotaWorker
{
    private readonly List<ITaskDefinition> _tasks;
    private readonly ILogger<SrotaWorker> _logger;
    private readonly Action<Exception, string>? _globalErrorHandler;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    
    public bool IsRunning { get; private set; }
```



**This is the ACTUAL ROBOT that does all the work!**

```csharp
private readonly List<ITaskDefinition> _tasks;
```
- **The to-do list** you gave the robot

```csharp
private readonly ILogger<SrotaWorker> _logger;
```
- **The diary** where the robot writes what it's doing

```csharp
private CancellationTokenSource? _cts;
```
- **The STOP button** for the robot

```csharp
public bool IsRunning { get; private set; }
```
- **Is the robot awake?** (Yes/No)

---

```csharp
public Task StartAsync(CancellationToken cancellationToken = default)
{
    if (IsRunning)
        throw new InvalidOperationException("Worker is already running");
    
    _logger.LogInformation("Starting Srota worker with {TaskCount} tasks", _tasks.Count);
    
    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    IsRunning = true;
    
    var taskExecutions = _tasks.Select(task => ExecuteTaskWithLogging(task, _cts.Token));
    _runningTask = Task.WhenAll(taskExecutions);
    
    return Task.CompletedTask;
}
```



**"Wake up the robot and start doing ALL the chores!"**

```csharp
if (IsRunning)
    throw new InvalidOperationException("Worker is already running");
```
- **Check**: "Are you already awake?"
- If YES → "You're already working! Don't press START twice!"

```csharp
_logger.LogInformation("Starting Srota worker with {TaskCount} tasks", _tasks.Count);
```
- **Write in diary**: "I'm waking up! I have 5 chores to do today"

```csharp
IsRunning = true;
```
- **Turn ON**: Robot's eyes light up! 🤖✨

```csharp
var taskExecutions = _tasks.Select(task => ExecuteTaskWithLogging(task, _cts.Token));
_runningTask = Task.WhenAll(taskExecutions);
```
- **Start ALL chores at the same time!**
- Like having multiple robot arms doing different jobs:
  - Arm 1: Waters plants
  - Arm 2: Checks mailbox
  - Arm 3: Listens for texts

---

```csharp
public async Task StopAsync(CancellationToken cancellationToken = default)
{
    if (!IsRunning) return;
    
    _logger.LogInformation("Stopping Srota worker");
    
    _cts?.Cancel();
    
    if (_runningTask != null)
    {
        try
        {
            await _runningTask;
        }
        catch (OperationCanceledException)
        {
            // Expected during cancellation
        }
    }
    
    IsRunning = false;
    _logger.LogInformation("Srota worker stopped");
}
```



**"Robot, go to sleep now!"**

```csharp
if (!IsRunning) return;
```
- **Check**: "Are you even awake?"
- If NO → "You're already asleep, silly!"

```csharp
_cts?.Cancel();
```
- **Press the STOP button** 🛑
- Tell all the robot arms: "Stop what you're doing!"

```csharp
await _runningTask;
```
- **Wait for the robot to finish** what it's doing
- Like waiting for it to put down the watering can

```csharp
IsRunning = false;
```
- **Turn OFF**: Robot's eyes go dark 🤖💤

---

## 🎬 Putting It All Together - A Story!

**Imagine you want a robot to help at home:**

```csharp
var robot = WorkerBuilder.Create()
    .AddPolling("Water plants", TimeSpan.FromDays(2))
        .Do(() => Console.WriteLine("💧 Watering plants!"))
    
    .AddPolling("Check mailbox", TimeSpan.FromMinutes(30))
        .Do(() => Console.WriteLine("📬 Checking mailbox!"))
    
    .AddEvent("Listen for texts", () => myPhone)
        .Do(message => Console.WriteLine($"📱 New text: {message}"))
    
    .Build();

await robot.StartAsync(); // Robot wakes up!
// Robot is now working...
await Task.Delay(TimeSpan.FromHours(1)); // Let it work for 1 hour
await robot.StopAsync(); // Robot goes to sleep!
```

**What happens:**

1. ⏰ **Robot wakes up** (`StartAsync`)
2. 🤖 **Starts 3 jobs at once:**
   - Waters plants every 2 days
   - Checks mailbox every 30 minutes
   - Listens for text messages constantly
3. ⏳ **Works for 1 hour**
4. 😴 **Goes to sleep** (`StopAsync`)

---

## 🎓 Summary - The Big Picture

1. **Abstractions** = The rules (what robots must know)
2. **Models** = What messages look like
3. **Builders** = How you give instructions to the robot
4. **Tasks** = Different types of jobs (repeating, listening, etc.)
5. **Core** = The actual robot that does the work

**It's like:**
- 📖 Writing a to-do list
- 🏗️ Building a robot
- ▶️ Pressing START
- 🤖 Robot does all the chores automatically!
- ⏹️ Pressing STOP when you're done

---

## 🎈 The Magic of Srota

Instead of YOU having to:
- ⏰ Remember to check mailbox every 30 minutes
- 📱 Keep your phone open waiting for texts
- 💧 Set alarms to water plants

**Srota does it ALL automatically!** 🎉

You just:
1. Tell it WHAT to do
2. Tell it WHEN to do it
3. Press START
4. Go play! 🎮

The robot handles everything! 🤖✨