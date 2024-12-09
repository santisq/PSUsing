using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;
using System.Threading;

[Cmdlet(VerbsOther.Use, "Object")]
[Alias("use")]
public sealed class UseObjectCommand : PSCmdlet, IDisposable
{
    private SteppablePipeline? _pipe;

    private CancellationTokenSource? _src;

    [Parameter(ValueFromPipeline = true)]
    public object? InputObject { get; set; }

    [Parameter(Position = 0)]
    [ValidateNotNullOrEmpty]
    public IDisposable? Disposable { get; set; }

    [Parameter(Mandatory = true, Position = 1)]
    public ScriptBlock ScriptBlock { get; set; } = null!;

    [Parameter]
    public SwitchParameter UseLocalScope { get; set; }

    [Parameter(ParameterSetName = "Token")]
    public SwitchParameter CancellationToken { get; set; }

    [Parameter(ParameterSetName = "Token")]
    [ValidateRange(1, int.MaxValue)]
    public int TimeoutSeconds { get; set; }

    protected override void BeginProcessing()
    {
        List<object> args = [ConvertToProcessBlockIfUnnamed(ScriptBlock)];

        StringBuilder builder = new StringBuilder()
            .Append("param($__sb");

        if (CancellationToken.IsPresent)
        {
            _src = new CancellationTokenSource(
                TimeoutSeconds > 0 ? TimeoutSeconds * 1000 : -1);
            builder.Append(", $__token");
            args.Add(_src.Token);
        }

        builder
            .Append(") ")
            .Append(UseLocalScope.IsPresent ? "." : "&")
            .Append(" $__sb")
            .Append(_src != null ? " $__token" : null);

        _pipe = ScriptBlock
            .Create(builder.ToString())
            .GetSteppablePipeline(CommandOrigin.Internal, [.. args]);

        _pipe.Begin(this);
    }

    protected override void ProcessRecord() => _pipe?.Process(InputObject);

    protected override void EndProcessing() => _pipe?.End();

    protected override void StopProcessing() => _src?.Cancel();

    private static ScriptBlock ConvertToProcessBlockIfUnnamed(ScriptBlock scriptBlock)
    {
        ScriptBlockAst sbAst = (ScriptBlockAst)scriptBlock.Ast;

        if (sbAst is not { BeginBlock: null, ProcessBlock: null })
        {
            return scriptBlock;
        }

        ScriptBlockAst newSbAst = new(
            scriptBlock.Ast.Extent,
            paramBlock: sbAst.ParamBlock?.Copy() as ParamBlockAst,
            beginBlock: null,
            processBlock: new NamedBlockAst(
                sbAst.EndBlock.Extent,
                TokenKind.Process,
                new StatementBlockAst(
                    sbAst.EndBlock.Extent,
                    sbAst.EndBlock.Statements.Select(s => s.Copy()).Cast<StatementAst>(),
                    []),
                unnamed: false),
            endBlock: null,
            dynamicParamBlock: null);

        return newSbAst.GetScriptBlock();
    }

    public void Dispose()
    {
        Disposable?.Dispose();
        _src?.Dispose();
        _pipe?.Dispose();
        GC.SuppressFinalize(this);
    }
}
