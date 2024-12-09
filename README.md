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

PSUsing is a little PowerShell Module that intends to solve an issue particularly found before the addition of the [`clean` block][cleanblock] in PowerShell 7.3, `Use-Object` offers a safe way to automatically clean-up resources while processing pipeline input, even when spanning the different [__Input processing methods__][inputmethods].

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

Compatible with __Windows PowerShell v5.1__ and [__PowerShell 7+__](https://github.com/PowerShell/PowerShell).

## Usage

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
[inputmethods]: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods?view=powershell-7.4#input-processing-methods
