$ErrorActionPreference = 'Stop'

Describe 'Use-Object' {
    BeforeAll {
        $moduleName = (Get-Item ([IO.Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
        $manifestPath = [IO.Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

        Import-Module $manifestPath

        class TestDisposable : IDisposable {
            [bool] $Disposed

            [void] Dispose() {
                $this.Disposed = $true
            }
        }
    }

    It 'Disposes an object' {
        Use-Object ($disposable = [TestDisposable]::new()) {
            $disposable.Disposed | Should -Be $false
        }

        $disposable.Disposed | Should -Be $true
    }


    It 'Disposes an object on terminating errors' {
        $disposable = [TestDisposable]::new()

        {
            Use-Object ($disposable) {
                throw
            }
        } | Should -Throw

        $disposable.Disposed | Should -Be $true
        $disposable = [TestDisposable]::new()

        {
            Use-Object ($disposable) {
                Write-Error 'an error'
            }
        } | Should -Throw

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

    It 'Runs in the local context with -UseLocalScope' {
        $index = 0
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) -UseLocalScope {
            $index++
        }

        $index | Should -BeExactly 11
        $disposable.Disposed | Should -Be $true
    }

    It 'Makes available a CancellationToken that gets cancelled on CTRL+C' {
        $job = Start-Job {
            Import-Module $using:manifestPath

            Use-Object -ScriptBlock {
                param($token)

                [System.Threading.Tasks.Task]::Delay(-1, $token).
                    GetAwaiter().GetResult()
            }
        }

        Start-Sleep 1

        { $job | Stop-Job -PassThru | Receive-Job -Wait -AutoRemoveJob } |
            Should -Not -Throw

        $job.State | Should -Be 'Stopped'
    }

    It 'Makes available a CancellationToken with Timeout' {
        $disposable = [TestDisposable]::new()

        {
            Use-Object ($disposable) -CancellationTimeout 5 {
                param($token)

                [System.Threading.Tasks.Task]::Delay(-1, $token).
                    GetAwaiter().GetResult()
            }
        } | Should -Throw

        $disposable.Disposed | Should -Be $true
        $disposable = [TestDisposable]::new()

        {
            0..10 | Use-Object ($disposable) -CancellationTimeout 5 {
                param($token)

                [System.Threading.Tasks.Task]::Delay(1000, $token).
                    GetAwaiter().GetResult()

                $_
            } | Should -BeExactly (0..4)
        } | Should -Throw

        $disposable.Disposed | Should -Be $true
    }

    It 'Disposes an object on StopUpstreamCommandsException' {
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) {
            $_
        } | Select-Object -First 5 | Should -BeExactly (0..4)

        $disposable.Disposed | Should -Be $true
    }
}
