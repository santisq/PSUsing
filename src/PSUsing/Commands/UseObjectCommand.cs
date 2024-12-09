using System;
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
    [Alias("sb")]
    public ScriptBlock ScriptBlock { get; set; } = null!;

    [Parameter]
    [Alias("ls")]
    public SwitchParameter UseLocalScope { get; set; }

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    [Alias("ct", "ts")]
    public int CancellationTimeout { get; set; } = -1;

    protected override void BeginProcessing()
    {
        if (CancellationTimeout != -1)
        {
            CancellationTimeout *= 1000;
        }

        _src = new CancellationTokenSource(CancellationTimeout);

        StringBuilder builder = new StringBuilder()
            .AppendLine("param($__sb, $__token)")
            .Append(UseLocalScope.IsPresent ? "." : "&")
            .Append(" $__sb $__token");

        _pipe = ScriptBlock
            .Create(builder.ToString())
            .GetSteppablePipeline(
                CommandOrigin.Internal,
                [ConvertToProcessBlockIfUnnamed(ScriptBlock), _src.Token]);

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
