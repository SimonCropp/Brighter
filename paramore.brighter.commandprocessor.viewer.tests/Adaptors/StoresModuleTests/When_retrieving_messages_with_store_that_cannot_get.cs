﻿using Nancy.Json;
using Nancy.Testing;
using NUnit.Framework;
using paramore.brighter.commandprocessor.messageviewer.Adaptors.API.Modules;
using paramore.brighter.commandprocessor.messageviewer.Adaptors.API.Resources;
using paramore.brighter.commandprocessor.messageviewer.Ports.ViewModelRetrievers;
using paramore.brighter.commandprocessor.viewer.tests.TestDoubles;

namespace paramore.brighter.commandprocessor.viewer.tests.Adaptors.StoresModuleTests
{
    [TestFixture]
    public class RetreiveMessageStoreReadFailureTests
    {
        private readonly string _storeUri = "/stores/storeName";
        private Browser _browser;
        private BrowserResponse _result;

        [SetUp]
        public void Establish()
        {
            _browser = new Browser(new ConfigurableBootstrapper(with =>
            {
                ConfigureStoreModuleForStoreError(with, MessageStoreViewerModelError.StoreMessageViewerGetException);
            }));
        }

        [Test]
        public void When_retrieving_messages_with_store_that_cannot_get()
        {
            _result = _browser.Get(_storeUri, with =>
                {
                    with.Header("accept", "application/json");
                    with.HttpRequest();
                })
                .Result;

            //should_return_500_Server_error
            _result.StatusCode.ShouldEqual(Nancy.HttpStatusCode.InternalServerError);
            //should_return_json
            _result.ContentType.ShouldContain("application/json");
            //should_return_error
            var serializer = new JavaScriptSerializer();
            var model = serializer.Deserialize<MessageViewerError>(_result.Body.AsString());

            model.ShouldNotBeNull();
            model.Message.ShouldContain("Unable");

        }

        private static void ConfigureStoreModuleForStoreError(
            ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator with,
            MessageStoreViewerModelError messageStoreViewerModelError)
        {
            var listViewRetriever = FakeActivationListModelRetriever.Empty();
            var storeRetriever = new FakeMessageStoreViewerModelRetriever(messageStoreViewerModelError);
            var messageRetriever = FakeMessageListViewModelRetriever.Empty();

            with.Module(new StoresNancyModule(listViewRetriever, storeRetriever, messageRetriever));
        }
    }
}