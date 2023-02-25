﻿using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.TestRunner;

public class TestDiagnosticContext
{
    public TestDiagnosticContext()
    {
        Diagnostics = new List<IDiagnosticMessage>();
        Errors = new List<IErrorMessage>();
    }

    public List<IErrorMessage> Errors { get; }
    public List<IDiagnosticMessage> Diagnostics { get; }

    public void SubscribeToEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Diagnostics.DiagnosticMessageEvent += DiagnosticMessageEvent;
        testMessageSink.Diagnostics.ErrorMessageEvent += ErrorMessageEvent;
    }

    public void UnsubscribeFromEvents(ref TestMessageSink testMessageSink)
    {
        testMessageSink.Diagnostics.DiagnosticMessageEvent -= DiagnosticMessageEvent;
        testMessageSink.Diagnostics.ErrorMessageEvent -= ErrorMessageEvent;
    }

    private void ErrorMessageEvent(MessageHandlerArgs<IErrorMessage> args)
    {
        if (args.Message is null)
            return;

        Errors.Add(args.Message);
    }

    private void DiagnosticMessageEvent(MessageHandlerArgs<IDiagnosticMessage> args)
    {
        if (args.Message is null)
            return;

        Diagnostics.Add(args.Message);
    }

    public string GetErrors()
    {
        return string.Join(Environment.NewLine, Errors.Select(error => error.Messages));
    }

    public string GetDiagnostics()
    {
        return string.Join(Environment.NewLine, Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}