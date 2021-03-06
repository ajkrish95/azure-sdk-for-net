﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Amqp;
using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework;

namespace Azure.Messaging.EventHubs.Tests
{
    /// <summary>
    ///   The suite of tests for the <see cref="ExceptionExtensions" />
    ///   class.
    /// </summary>
    ///
    [TestFixture]
    public class ExceptionExtensionsTests
    {
        /// <summary>
        ///   The set of test cases for "normal" exceptions that do not get
        ///   translated.
        /// </summary>
        ///
        public static IEnumerable<object[]> GeneralExceptionCases()
        {
            yield return new[] { new ArgumentNullException("blah") };
            yield return new[] { new EventHubsException(false, "thing") };
            yield return new[] { new TimeoutException() };
            yield return new[] { new TaskCanceledException() };
            yield return new[] { new Exception() };
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        public void TranslateServiceExceptionValidatesTheInstance()
        {
            Assert.That(() => ((Exception)null).TranslateServiceException("dummy"), Throws.ArgumentNullException);
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        public void TranslateServiceExceptionTranslatesAmqpExceptions()
        {
            var eventHub = "someHub";
            var exception = AmqpError.CreateExceptionForError(new Error { Condition = AmqpError.ServerBusyError }, eventHub);
            var translated = exception.TranslateServiceException(eventHub);

            Assert.That(translated, Is.Not.Null, "An exception should have been returned.");

            var eventHubsException = translated as EventHubsException;
            Assert.That(eventHubsException, Is.Not.Null, "The exception type should be appropriate for the `Server Busy` scenario.");
            Assert.That(eventHubsException.Reason, Is.EqualTo(EventHubsException.FailureReason.ServiceBusy), "The exception reason should indicate `Server Busy`.");
            Assert.That(eventHubsException.EventHubName, Is.EqualTo(eventHub), "The Event Hub name should match.");
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        public void TranslateServiceExceptionTranslatesOperationCanceledWithoutEmbeddedExceptions()
        {
            var eventHub = "someHub";
            var exception = new OperationCanceledException();
            var translated = exception.TranslateServiceException(eventHub);

            Assert.That(translated, Is.Not.Null, "An exception should have been returned.");

            var eventHubsException = translated as EventHubsException;
            Assert.That(eventHubsException, Is.Not.Null, "The exception type should be appropriate for the `Server Busy` scenario.");
            Assert.That(eventHubsException.Reason, Is.EqualTo(EventHubsException.FailureReason.ServiceTimeout), "The exception reason should indicate a service timeout.");
            Assert.That(eventHubsException.EventHubName, Is.EqualTo(eventHub), "The Event Hub name should match.");
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        public void TranslateServiceExceptionTranslatesOperationCanceledWithEmbeddedAmqpException()
        {
            var eventHub = "someHub";
            var exception = new OperationCanceledException("oops", AmqpError.CreateExceptionForError(new Error { Condition = AmqpError.ServerBusyError }, eventHub));
            var translated = exception.TranslateServiceException(eventHub);

            Assert.That(translated, Is.Not.Null, "An exception should have been returned.");

            var eventHubsException = translated as EventHubsException;
            Assert.That(eventHubsException, Is.Not.Null, "The exception type should be appropriate for the `Server Busy` scenario.");
            Assert.That(eventHubsException.Reason, Is.EqualTo(EventHubsException.FailureReason.ServiceBusy), "The exception reason should indicate `Server Busy`.");
            Assert.That(eventHubsException.EventHubName, Is.EqualTo(eventHub), "The Event Hub name should match.");
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        public void TranslateServiceExceptionTranslatesOperationCanceledWithEmbeddedGeneralException()
        {
            var eventHub = "someHub";
            var embedded = new ArgumentException();
            var exception = new OperationCanceledException("oops", embedded);
            var translated = exception.TranslateServiceException(eventHub);

            Assert.That(translated, Is.Not.Null, "An exception should have been returned.");
            Assert.That(translated, Is.SameAs(embedded), "The embedded (inner) exception should have been returned.");
        }

        /// <summary>
        ///   Verifies functionality of the <see cref="ExceptionExtensions.TranslateServiceException" />
        ///   method.
        /// </summary>
        ///
        [Test]
        [TestCaseSource(nameof(GeneralExceptionCases))]
        public void TranslateServiceExceptionDoesNotTranslateGeneralExceptions(Exception generalException)
        {
            var eventHub = "someHub";
            var translated = generalException.TranslateServiceException(eventHub);

            Assert.That(translated, Is.Not.Null, "An exception should have been returned.");
            Assert.That(translated, Is.SameAs(generalException), "The general exception should have been returned.");
        }
    }
}
