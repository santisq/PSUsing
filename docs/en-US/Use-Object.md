---
external help file: PSUsing.dll-Help.xml
Module Name: PSUsing
online version:
schema: 2.0.0
---

# Use-Object

## SYNOPSIS

A C# `using` statement like cmdlet with extended features.

## SYNTAX

```powershell
Use-Object
    [-InputObject <Object>]
    [[-Disposable] <IDisposable>]
    [-ScriptBlock] <ScriptBlock>
    [-UseLocalScope]
    [-CancellationTimeout <Int32>]
    [<CommonParameters>]
```

## DESCRIPTION

`Use-Object` cmdlet offers an easy way to invoke a script block that can span different [__Input processing methods__](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods#input-processing-methods) and automatically clean-up resources when completed.

__Resource cleanup is enforced for the same scenarios as the ones detailed in [`clean` block](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods#clean)__:

- When the pipeline execution finishes normally without terminating error.
- When the pipeline execution is interrupted due to terminating error.
- When the pipeline is halted by `Select-Object -First`.
- When the pipeline is being stopped by <kbd>CTRL + C</kbd> or `StopProcessing()`.

## EXAMPLES

### Example 1: Use a disposable object

```powershell
use ($myobj = [DisposableObject]::new()) {
   # do stuff with:
   $myObj
}
```

### Example 2: Use a disposable object while processing input

```powershell
0..10 | use ($myobj = [DisposableObject]::new()) {
   $myObj.DoStuff($_)
}
```

### Example 3: Span different processing methods

```powershell
0..10 | use ($myobj = [DisposableObject]::new()) {
   begin { 'begin' }
   process { $myObj.DoStuff($_) }
   end { 'end' }
}
```

### Example 4: Invoke using a `CancellationToken`

```powershell
# can CTRL+C out of this
use -sb {
   param($token)
   
   [System.Threading.Tasks.Task]::Delay(-1, $token).Wait()
}

# can stop this
$job = Start-Job {
   use -sb {
      param($token)

      [System.Threading.Tasks.Task]::Delay(-1, $token).Wait()
   }
}

Start-Sleep 1
$job | Stop-Job
```

A [`CancellationToken`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) is available for .NET methods that support it.
The cancellation source is tied to the cmdlet's [`StopProcessing()` method](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.cmdlet.stopprocessing).

## PARAMETERS

### -Disposable

An object implementing the `IDisposable` interface to be disposed after completing the invocation.

```yaml
Type: IDisposable
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

Specifies the input objects to be processed in the ScriptBlock.

> [!NOTE]
>
> This parameter is intended to be bound from pipeline.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ScriptBlock

Specifies the operation that is performed on each input object.
This script block is run for every object in the pipeline.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases: sb

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseLocalScope

By default the script block is invoked in a child scope (`&`), this switch enables the cmdlet to run in the current scope (`.`).

> [!TIP]
> For more information see:
>
> - [Dot sourcing operator `.`](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_operators#call-operator-)
> - [Call operator `&`](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_operators#call-operator-)

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: ls

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -CancellationTimeout

Specifies a timeout __in seconds__ for the Cancellation Token.

> [!NOTE]
> The default timeout is `-1`, meaning no timeout. In this case the token source is cancelled only on `StopProcessing()` (i.e.: <kbd>CTRL + C</kbd>).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: ct, ts

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters.
For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
