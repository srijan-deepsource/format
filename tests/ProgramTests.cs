﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.CodeAnalysis.Tools.Commands;
using Xunit;

namespace Microsoft.CodeAnalysis.Tools.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ExitCodeIsOneWithCheckAndAnyFilesFormatted()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 1, fileCount: 0, exitCode: 0);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: true);

            Assert.Equal(FormatCommandCommon.CheckFailedExitCode, exitCode);
        }

        [Fact]
        public void ExitCodeIsZeroWithCheckAndNoFilesFormatted()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 0, fileCount: 0, exitCode: 42);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: true);

            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void ExitCodeIsSameWithoutCheck()
        {
            var formatResult = new WorkspaceFormatResult(filesFormatted: 0, fileCount: 0, exitCode: 42);
            var exitCode = FormatCommandCommon.GetExitCode(formatResult, check: false);

            Assert.Equal(formatResult.ExitCode, exitCode);
        }

        [Fact]
        public void CommandLine_OptionsAreParsedCorrectly()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] {
                "--no-restore",
                "--include", "include1", "include2",
                "--exclude", "exclude1", "exclude2",
                "--verify-no-changes",
                "--binarylog", "binary-log-path",
                "--report", "report",
                "--verbosity", "detailed",
                "--include-generated"});

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.Equal(0, result.UnmatchedTokens.Count);
            Assert.Equal(0, result.UnparsedTokens.Count);
            result.GetValueForOption<bool>("--no-restore");
            Assert.Collection(result.GetValueForOption<IEnumerable<string>>("--include"),
                i0 => Assert.Equal("include1", i0),
                i1 => Assert.Equal("include2", i1));
            Assert.Collection(result.GetValueForOption<IEnumerable<string>>("--exclude"),
                i0 => Assert.Equal("exclude1", i0),
                i1 => Assert.Equal("exclude2", i1));
            Assert.True(result.GetValueForOption<bool>("--verify-no-changes"));
            Assert.Equal("binary-log-path", result.GetValueForOption<string>("--binarylog"));
            Assert.Equal("report", result.GetValueForOption<string>("--report"));
            Assert.Equal("detailed", result.GetValueForOption<string>("--verbosity"));
            Assert.True(result.GetValueForOption<bool>("--include-generated"));
        }

        [Fact]
        public void CommandLine_ProjectArgument_Simple()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.Equal("workspaceValue", result.GetValueForArgument<string>(FormatCommandCommon.SlnOrProjectArgument));
        }

        [Fact]
        public void CommandLine_ProjectArgument_WithOption_AfterArgument()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue", "--verbosity", "detailed" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.Equal("workspaceValue", result.GetValueForArgument<string>(FormatCommandCommon.SlnOrProjectArgument));
            Assert.Equal("detailed", result.GetValueForOption<string>("--verbosity"));
        }

        [Fact]
        public void CommandLine_ProjectArgument_WithOption_BeforeArgument()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--verbosity", "detailed", "workspaceValue" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.Equal("workspaceValue", result.GetValueForArgument<string>(FormatCommandCommon.SlnOrProjectArgument));
            Assert.Equal("detailed", result.GetValueForOption<string>("--verbosity"));
        }

        [Fact]
        public void CommandLine_ProjectArgument_FailsIfSpecifiedTwice()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "workspaceValue1", "workspaceValue2" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_FolderValidation_FailsIfFixAnalyzersSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--folder", "--fix-analyzers" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_FolderValidation_FailsIfFixStyleSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--folder", "--fix-style" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_FolderValidation_FailsIfNoRestoreSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "whitespace", "--folder", "--no-restore" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_BinaryLog_DoesNotFailIfPathNotSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--binarylog" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.True(result.WasOptionUsed("--binarylog"));
        }

        [Fact]
        public void CommandLine_BinaryLog_DoesNotFailIfPathIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--binarylog", "log" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
            Assert.True(result.WasOptionUsed("--binarylog"));
        }

        [Fact]
        public void CommandLine_BinaryLog_FailsIfFolderIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "whitespace", "--folder", "--binarylog" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_Diagnostics_FailsIfDiagnosticNoSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics" });

            // Assert
            Assert.Equal(1, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_Diagnostics_DoesNotFailIfDiagnosticIsSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics", "RS0016" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
        }

        [Fact]
        public void CommandLine_Diagnostics_DoesNotFailIfMultipleDiagnosticAreSpecified()
        {
            // Arrange
            var sut = RootFormatCommand.GetCommand();

            // Act
            var result = sut.Parse(new[] { "--diagnostics", "RS0016", "RS0017", "RS0018" });

            // Assert
            Assert.Equal(0, result.Errors.Count);
        }
    }
}
