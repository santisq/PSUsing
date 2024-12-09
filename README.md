<h1 align="center">PSUsing</h1>

<div align="center">
<sub>

[C# `using` statement][csusing] for PowerShell

</sub>
<br/>

[![build][build_badge]][build_ref]
[![codecov][codecov_badge]][codecov_ref]
[![PowerShell Gallery][gallery_badge]][gallery_ref]
[![LICENSE][license_badge]][license_ref]

</div>

PSUsing is a little PowerShell Module that offers an easy way to invoke a script block that can span different [__Input processing methods__][inputmethods] and automatically clean-up resources when completed.

__Resource cleanup is enforced for the same scenarios as the ones detailed in [`clean` block][cleanblock]__:

- When the pipeline execution finishes normally without terminating error.
- When the pipeline execution is interrupted due to terminating error.
- When the pipeline is halted by `Select-Object -First`.
- When the pipeline is being stopped by <kbd>CTRL + C</kbd> or `StopProcessing()`.

## Documentation

Check out [__the docs__](./docs/en-US/Use-Object.md) for information about how to use this Module.

## Installation

### Gallery

The module is available through the [PowerShell Gallery][gallery_ref]:

```powershell
Install-Module PSUsing -Scope CurrentUser
```

### Source

```powershell
git clone 'https://github.com/santisq/PSUsing.git'
Set-Location ./PSUsing
./build.ps1
```

## Requirements

Compatible with __Windows PowerShell v5.1__ and [__PowerShell 7+__][psgithub].

## Usage

The most common pattern would be:

```powershell
use ($myobj = [DisposableObject]::new()) {
   # do stuff with:
   $myObj
}
```

The cmdlet can also process pipeline input, for example:

```powershell
0..10 | use ($myobj = [DisposableObject]::new()) {
   $myObj.DoStuff($_)
}
```

And can span different processing methods:

```powershell
0..10 | use ($myobj = [DisposableObject]::new()) {
   begin { 'begin' }
   process { $myObj.DoStuff($_) }
   end { 'end' }
}
```

A [`CancellationToken`][cancellation] is available for .NET methods that support it.
The cancellation source is tied to the cmdlet's [`StopProcessing()` method][stopprocessing]:

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

The token can be cancelled on a timeout too:

```powershell
# would throw a `TaskCanceledException` after 5 seconds
use -ct 5 -sb {
   param($token)

   [System.Threading.Tasks.Task]::Delay(-1, $token).GetAwaiter().GetResult()
}
```

## Contributing

Contributions are welcome, if you wish to contribute, fork this repository and submit a pull request with the changes.

[codecov_badge]: https://codecov.io/gh/santisq/PSUsing/branch/main/graph/badge.svg?token=b51IOhpLfQ
[codecov_ref]: https://codecov.io/gh/santisq/PSUsing
[build_badge]: https://github.com/santisq/PSUsing/actions/workflows/ci.yml/badge.svg
[build_ref]: https://github.com/santisq/PSUsing/actions/workflows/ci.yml
[gallery_badge]: https://img.shields.io/powershellgallery/dt/PSUsing?color=%23008FC7
[gallery_ref]: https://www.powershellgallery.com/packages/PSUsing
[license_badge]: https://img.shields.io/github/license/santisq/PSUsing
[license_ref]: https://github.com/santisq/PSUsing/blob/main/LICENSE
[cleanblock]: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods#clean
[csusing]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/using
[inputmethods]: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods#input-processing-methods
[psgithub]: https://github.com/PowerShell/PowerShell
[cancellation]: https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken
[stopprocessing]: https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.cmdlet.stopprocessing
