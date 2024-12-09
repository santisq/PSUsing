$ErrorActionPreference = 'Stop'

$moduleName = (Get-Item ([IO.Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [IO.Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath

class TestDisposable : IDisposable {
    [bool] $Disposed

    [void] Dispose() {
        $this.Disposed = $true
    }
}

Describe 'Use-Object' {
    It 'Disposes an object' {
        Use-Object ($disposable = [TestDisposable]::new()) {
            $disposable.Disposed | Should -Be $false
        }

        $disposable.Disposed | Should -Be $true
    }

    It 'Takes pipeline input' {
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) { $_ } |
            Should -BeExactly (0..10)

        $disposable.Disposed | Should -Be $true
    }

    It 'Supports named blocks' {
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) {
            begin { 'begin' }
            process { $_ }
            end { 'end' }
        } | Should -BeExactly @('begin'; (0..10); 'end')

        $disposable.Disposed | Should -Be $true
    }

    It 'Executs in the local context with -UseLocalScope' {
        $index = 0
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) -UseLocalScope {
            $index++
        }

        $index | Should -BeExactly 11
        $disposable.Disposed | Should -Be $true
    }
}
