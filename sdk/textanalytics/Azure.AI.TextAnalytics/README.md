# Azure Cognitive Services Text Analytics client library for .NET
Azure Cognitive Services Text Analytics is a cloud service that provides advanced natural language processing over raw text, and includes six main functions: 
* Language Detection
* Sentiment Analysis
* Key Phrase Extraction
* Named Entity Recognition
* Recognition of Personally Identifiable Information 
* Linked Entity Recognition

[Source code][textanalytics_client_src] | [Package (NuGet)][textanalytics_nuget_package] | [API reference documentation][textanalytics_refdocs] | [Product documentation][textanalytics_docs] | [Samples][textanalytics_samples]

## Getting started

### Prerequisites
* An [Azure subscription][azure_sub].
* An existing Cognitive Services or Text Analytics resource.

### Create a Cognitive Services or Text Analytics resource
Text Analytics supports both [multi-service and single-service access][cognitive_resource_portal]. Create a Cognitive Services resource if you plan to access multiple cognitive services under a single endpoint/key. For Text Analytics access only, create a Text Analytics resource.

You can create either resource using: 

**Option 1:** [Azure Portal][cognitive_resource_portal].

**Option 2:** [Azure CLI][cognitive_resource_cli]. 

Below is an example of how you can create a Text Analytics resource using the CLI:

```PowerShell
# Create a new resource group to hold the text analytics resource -
# if using an existing resource group, skip this step
az group create --name <your-resource-name> --location <location>
```

```PowerShell
# Create text analytics
az cognitiveservices account create \
    --name <your-resource-name> \
    --resource-group <your-resource-group-name> \
    --kind TextAnalytics \
    --sku <sku> \
    --location <location> \
    --yes
```
For more information about creating the resource or how to get the location and sku information see [here][cognitive_resource_cli].

### Install the package
Install the Azure Text Analytics client library for .NET with [NuGet][nuget]:

```PowerShell
Install-Package Azure.AI.TextAnalytics -Version 1.0.0-preview.2
```

### Authenticate the client
In order to interact with the Text Analytics service, you'll need to create an instance of the [TextAnalyticsClient][textanalytics_client_class] class. You will need an **endpoint**, and either an **API key** or ``TokenCredential`` to instantiate a client object.  For more information regarding authenticating with cognitive services, see [Authenticate requests to Azure Cognitive Services][cognitive_auth].

#### Get API Key
You can get the `endpoint` and `API key` from the Cognitive Services resource or Text Analytics resource information in the [Azure Portal][azure_portal].

Alternatively, use the [Azure CLI][azure_cli] snippet below to get the API key from the Text Analytics resource.

```PowerShell
az cognitiveservices account keys list --resource-group <your-resource-group-name> --name <your-resource-name>
```

#### Create TextAnalyticsClient with API Key Credential
Once you have the value for the API key, create a `TextAnalyticsApiKeyCredential`. This will allow you to
update the API key by using the `UpdateCredential` method without creating a new client.

With the value of the endpoint and a `TextAnalyticsApiKeyCredential`, you can create the [TextAnalyticsClient][textanalytics_client_class]:

```C# Snippet:CreateTextAnalyticsClient
string endpoint = "<endpoint>";
string apiKey = "<apiKey>";
var credential = new TextAnalyticsApiKeyCredential(apiKey);
var client = new TextAnalyticsClient(new Uri(endpoint), credential);
```

#### Create TextAnalyticsClient with Azure Active Directory Credential

Client API key authentication is used in most of the examples in this getting started guide, but you can also authenticate with Azure Active Directory using the [Azure Identity library][azure_identity].  Note that regional endpoints do not support AAD authentication. Create a [custom subdomain][custom_subdomain] for your resource in order to use this type of authentication.  

To use the [DefaultAzureCredential][DefaultAzureCredential] provider shown below,
or other credential providers provided with the Azure SDK, please install the Azure.Identity package:

```PowerShell
Install-Package Azure.Identity
```

You will also need to [register a new AAD application][register_aad_app] and [grant access][aad_grant_access] to Text Analytics by assigning the `"Cognitive Services User"` role to your service principal.

Set the values of the client ID, tenant ID, and client secret of the AAD application as environment variables: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_CLIENT_SECRET.

```C# Snippet:CreateTextAnalyticsClientTokenCredential
string endpoint = "<endpoint>";
var client = new TextAnalyticsClient(new Uri(endpoint), new DefaultAzureCredential());
```

## Key concepts

### TextAnalyticsClient
A `TextAnalyticsClient` is the primary interface for developers using the Text Analytics client library.  It provides both synchronous and asynchronous operations to access a specific use of Text Analytics, such as language detection or key phrase extraction. 

### Text Input
A **text input**, sometimes called a **document**, is a single unit of input to be analyzed by the predictive models in the Text Analytics service.  Operations on `TextAnalyticsClient` may take a single text input or a collection of inputs to be analyzed as a batch.
For text input length limits, maximum batch size, and supported text encoding see [here][data_limits].

### Return value
Return values, such as `AnalyzeSentimentResult`, is the result of a Text Analytics operation, containing a prediction or predictions about a single text input.  An operation's return value also may optionally include information about the input document and how it was processed.

### Return value Collection
A Return value collection, such as `AnalyzeSentimentResultCollection`, is a collection of operation results, where each corresponds to one of the text inputs provided in the input batch.  A text input and its result will have the same index in the input and result collections. The return value also contains a `HasError` property that allows to identify if an operation executed was succesful or unsuccesful for the given text input. It may optionally include information about the input batch and how it was processed.

 ### Operation on multiple text inputs
For each supported operation, `TextAnalyticsClient` provides a method that accepts a batch of text inputs as strings, or a batch of either `TextDocumentInput` or `DetectLanguageInput` objects. This methods allow callers to give each document a unique ID, indicate that the documents in the batch are written in different languages, or provide a country hint about the language of the document.

**Note:** It is recommended to use the batch methods when working on production environments as they allow you to send one request with multiple text inputs. This is more performant than sending a request per each text input.

## Examples
The following section provides several code snippets using the `client` [created above](#create-textanalyticsclient), and covers the main functions of Text Analytics.

### Sync examples
* [Detect Language](#detect-language)
* [Analyze Sentiment](#analyze-sentiment)
* [Extract Key Phrases](#extract-key-phrases)
* [Recognize Entities](#recognize-entities)
* [Recognize PII Entities](#recognize-pii-entities)
* [Recognize Linked Entities](#recognize-linked-entities)

### Async examples
* [Detect Language Asynchronously](#detect-language-asynchronously)
* [Recognize Entities Asyncronously](#recognize-entities-asynchronously)

### Detect Language
Run a Text Analytics predictive model to determine the language that the passed-in input text or batch of input text documents are written in.

```C# Snippet:DetectLanguage
string input = "Este documento está en español.";

DetectedLanguage language = client.DetectLanguage(input);

Console.WriteLine($"Detected language {language.Name} with confidence {language.Score:0.00}.");
```
For samples on using the production recommended option `DetectLanguageBatch` see [here](#detect-language-1).

Please refer to the service documentation for a conceptual discussion of [language detection][language_detection].

### Analyze Sentiment
Run a Text Analytics predictive model to identify the positive, negative, neutral or mixed sentiment contained in the passed-in input text or batch of input text documents.

```C# Snippet:AnalyzeSentiment
string input = "That was the best day of my life!";

DocumentSentiment docSentiment = client.AnalyzeSentiment(input);

Console.WriteLine($"Sentiment was {docSentiment.Sentiment}, with confidence scores: ");
Console.WriteLine($"    Positive confidence score: {docSentiment.ConfidenceScores.Positive:0.00}.");
Console.WriteLine($"    Neutral confidence score: {docSentiment.ConfidenceScores.Neutral:0.00}.");
Console.WriteLine($"    Negative confidence score: {docSentiment.ConfidenceScores.Negative:0.00}.");
```
For samples on using the production recommended option `AnalyzeSentimentBatch` see [here](#analyze-sentiment-1).

Please refer to the service documentation for a conceptual discussion of [sentiment analysis][sentiment_analysis].

### Extract Key Phrases
Run a model to identify a collection of significant phrases found in the passed-in input text or batch of input text documents.

```C# Snippet:ExtractKeyPhrases
string input = "My cat might need to see a veterinarian.";

Response<IReadOnlyCollection<string>> response = client.ExtractKeyPhrases(input);
IEnumerable<string> keyPhrases = response.Value;

Console.WriteLine($"Extracted {keyPhrases.Count()} key phrases:");
foreach (string keyPhrase in keyPhrases)
{
    Console.WriteLine(keyPhrase);
}
```
For samples on using the production recommended option `ExtractKeyPhrasesBatch` see [here](#extract-key-phrases-1).

Please refer to the service documentation for a conceptual discussion of [key phrase extraction][key_phrase_extraction].

### Recognize Entities
Run a predictive model to identify a collection of named entities in the passed-in input text or batch of input text documents and categorize those entities into categories such as person, location, or organization.  For more information on available categories, see [Text Analytics Named Entity Categories][named_entities_categories].

```C# Snippet:RecognizeEntities
string input = "Microsoft was founded by Bill Gates and Paul Allen.";

Response<IReadOnlyCollection<CategorizedEntity>> response = client.RecognizeEntities(input);
IEnumerable<CategorizedEntity> entities = response.Value;

Console.WriteLine($"Recognized {entities.Count()} entities:");
foreach (CategorizedEntity entity in entities)
{
    Console.WriteLine($"Text: {entity.Text}, Category: {entity.Category}, SubCategory: {entity.SubCategory}, Score: {entity.Score}, Offset: {entity.Offset}, Length: {entity.Length}");
}
```
For samples on using the production recommended option `RecognizeEntitiesBatch` see [here](#recognize-entities-1).

Please refer to the service documentation for a conceptual discussion of [named entity recognition][named_entity_recognition].

### Recognize PII Entities
Run a predictive model to identify a collection of entities containing Personally Identifiable Information found in the passed-in input text or batch of input text documents, and categorize those entities into categories such as US social security number, drivers license number, or credit card number.

```C# Snippet:RecognizePiiEntities
string input = "A developer with SSN 555-55-5555 whose phone number is 555-555-5555 is building tools with our APIs.";

Response<IReadOnlyCollection<PiiEntity>> response = client.RecognizePiiEntities(input);
IEnumerable<PiiEntity> entities = response.Value;

Console.WriteLine($"Recognized {entities.Count()} PII entit{(entities.Count() > 1 ? "ies" : "y")}:");
foreach (PiiEntity entity in entities)
{
    Console.WriteLine($"Text: {entity.Text}, Category: {entity.Category}, SubCategory: {entity.SubCategory}, Score: {entity.Score}, Offset: {entity.Offset}, Length: {entity.Length}");
}
```

For samples on using the production recommended option `RecognizePiiEntitiesBatch` see [here](#recognize-pii-entities-1).

### Recognize Linked Entities
Run a predictive model to identify a collection of entities found in the passed-in input text or batch of input text documents, and include information linking the entities to their corresponding entries in a well-known knowledge base.

```C# Snippet:RecognizeLinkedEntities
string input = "Microsoft was founded by Bill Gates and Paul Allen.";

Response<IReadOnlyCollection<LinkedEntity>> response = client.RecognizeLinkedEntities(input);
IEnumerable<LinkedEntity> linkedEntities = response.Value;

Console.WriteLine($"Extracted {linkedEntities.Count()} linked entit{(linkedEntities.Count() > 1 ? "ies" : "y")}:");
foreach (LinkedEntity linkedEntity in linkedEntities)
{
    Console.WriteLine($"Name: {linkedEntity.Name}, Language: {linkedEntity.Language}, Data Source: {linkedEntity.DataSource}, Url: {linkedEntity.Url.ToString()}, Entity Id in Data Source: {linkedEntity.DataSourceEntityId}");
    foreach (LinkedEntityMatch match in linkedEntity.Matches)
    {
        Console.WriteLine($"    Match Text: {match.Text}, Score: {match.Score:0.00}, Offset: {match.Offset}, Length: {match.Length}.");
    }
}
```
For samples on using the production recommended option `RecognizeLinkedEntitiesBatch` see [here](#recognize-linked-entities-1).

Please refer to the service documentation for a conceptual discussion of [entity linking][named_entity_recognition].

### Detect Language Asynchronously
Run a Text Analytics predictive model to determine the language that the passed-in input text or batch of input text documents are written in.

```C# Snippet:DetectLanguageAsync
string input = "Este documento está en español.";

DetectedLanguage language = await client.DetectLanguageAsync(input);

Console.WriteLine($"Detected language {language.Name} with confidence {language.Score:0.00}.");
```

### Recognize Entities Asynchronously
Run a predictive model to identify a collection of named entities in the passed-in input text or batch of input text documents and categorize those entities into categories such as person, location, or organization.  For more information on available categories, see [Text Analytics Named Entity Categories][named_entities_categories].

```C# Snippet:RecognizeEntitiesAsync
string input = "Microsoft was founded by Bill Gates and Paul Allen.";

Response<IReadOnlyCollection<CategorizedEntity>> response = await client.RecognizeEntitiesAsync(input);
IEnumerable<CategorizedEntity> entities = response.Value;

Console.WriteLine($"Recognized {entities.Count()} entities:");
foreach (CategorizedEntity entity in entities)
{
    Console.WriteLine($"Text: {entity.Text}, Category: {entity.Category}, SubCategory: {entity.SubCategory}, Score: {entity.Score}, Offset: {entity.Offset}, Length: {entity.Length}");
}
```

## Troubleshooting

### General
When you interact with the Cognitive Services Text Analytics client library using the .NET SDK, errors returned by the service correspond to the same HTTP status codes returned for [REST API][textanalytics_rest_api] requests.

For example, if you submit a batch of text document inputs containing duplicate document ids, a `400` error is returned, indicating "Bad Request".

```C# Snippet:BadRequest
try
{
    DetectedLanguage result = client.DetectLanguage(input);
}
catch (RequestFailedException e)
{
    Console.WriteLine(e.ToString());
}
```

You will notice that additional information is logged, like the client request ID of the operation.

```
Message:
    Azure.RequestFailedException:
    Status: 400 (Bad Request)

Content:
    {"error":{"code":"InvalidRequest","innerError":{"code":"InvalidDocument","message":"Request contains duplicated Ids. Make sure each document has a unique Id."},"message":"Invalid document in request."}}

Headers:
    Transfer-Encoding: chunked
    x-aml-ta-request-id: 146ca04a-af54-43d4-9872-01a004bee5f8
    X-Content-Type-Options: nosniff
    x-envoy-upstream-service-time: 6
    apim-request-id: c650acda-2b59-4ff7-b96a-e316442ea01b
    Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
    Date: Wed, 18 Dec 2019 16:24:52 GMT
    Content-Type: application/json; charset=utf-8
```

### Setting up console logging
The simplest way to see the logs is to enable the console logging.
To create an Azure SDK log listener that outputs messages to console use AzureEventSourceListener.CreateConsoleLogger method.

```
// Setup a listener to monitor logged events.
using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
```

To learn more about other logging mechanisms see [here][logging].

## Next steps

Samples showing how to use the Cognitive Services Text Analytics library are available in this GitHub repository.
Samples are provided for each main functional area, and for each area, samples are provided for analyzing a single text input, a collection of text input strings, and a collection of text document inputs.

### Detect Language
* [Sample1_DetectLanguage.cs][detect_language_sample0] - Detect the language of a single text input.
* [Sample1_DetectLanguageBatchConvenience.cs][detect_language_sample1] - Detect the language of each input in a collection of text input strings.
* [Sample1_DetectLanguageBatch.cs][detect_language_sample2] - Detect the language of each input in a collection of text document inputs.
* [Sample1_DetectLanguageAsync.cs][detect_language_sample_async] - Make an asynchronous call to detect the language of a single text input.

### Analyze Sentiment
* [Sample2_AnalyzeSentiment.cs][analyze_sentiment_sample0] - Analyze sentiment in a single text input.
* [Sample2_AnalyzeSentimentBatchConvenience.cs][analyze_sentiment_sample1] - Analyze sentiment in a collection of text input strings.
* [Sample2_AnalyzeSentimentBatch.cs][analyze_sentiment_sample2] - Analyze sentiment in a collection of text document inputs.

### Extract Key Phrases
* [Sample3_ExtractKeyPhrases.cs][extract_key_phrases_sample0] - Extract key phrases from a single text input.
* [Sample3_ExtractKeyPhrasesBatchConvenience.cs][extract_key_phrases_sample1] - Extract key phrases from a each input in a collection of text input strings.
* [Sample3_ExtractKeyPhrasesBatch.cs][extract_key_phrases_sample2] - Extract key phrases from a each input in a collection of text document inputs.

### Recognize Entities
* [Sample4_RecognizeEntities.cs][recognize_entities_sample0] - Recognize entities in a single text input.
* [Sample4_RecognizeEntitiesBatchConvenience.cs][recognize_entities_sample1] - Recognize entities in each input in a collection of text input strings.
* [Sample4_RecognizeEntitiesBatch.cs][recognize_entities_sample2] - Recognize entities in each input in a collection of text document inputs.
* [Sample4_RecognizeEntitiesAsync.cs][recognize_entities_sample_async] - Make an asynchronous call to detect the language of a single text input.

### Recognize PII Entities
* [Sample5_RecognizePiiEntities.cs][recognize_pii_entities_sample0] - Recognize entities containing Personally Identifiable Information in a single text input.
* [Sample5_RecognizePiiEntitiesBatch.cs][recognize_pii_entities_sample1] - Recognize entities containing Personally Identifiable Information in each input in a collection of document strings.
* [Sample5_RecognizePiiEntitiesBatchConvenience.cs][recognize_pii_entities_sample2] - Recognize entities containing Personally Identifiable Information in each input in a collection of text input string.
### Recognize Linked Entities
* [Sample6_RecognizeLinkedEntities.cs][recognize_linked_entities_sample0] - Recognize linked entities in a single text input.
* [Sample6_RecognizeLinkedEntitiesBatchConvenience.cs][recognize_linked_entities_sample1] - Recognize linked entities in each input in a collection of text input strings.
* [Sample6_RecognizeLinkedEntitiesBatch.cs][recognize_linked_entities_sample2] - Recognize linked entities in each input in a collection of text document inputs.

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [cla.microsoft.com][cla].

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct][code_of_conduct]. For more information see the [Code of Conduct FAQ][coc_faq] or contact [opencode@microsoft.com][coc_contact] with any additional questions or comments.

![Impressions](https://azure-sdk-impressions.azurewebsites.net/api/impressions/azure-sdk-for-net%2Fsdk%2Ftextanalytics%2FAzure.AI.TextAnalytics%2FREADME.png)


<!-- LINKS -->
[textanalytics_client_src]: src
[textanalytics_docs]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/
[textanalytics_refdocs]: https://aka.ms/azsdk-net-textanalytics-ref-docs
[textanalytics_nuget_package]: https://www.nuget.org/packages/Azure.AI.TextAnalytics
[textanalytics_samples]: tests/samples
[textanalytics_rest_api]: https://westus.dev.cognitive.microsoft.com/docs/services/TextAnalytics-v3-0-Preview-1/operations/Languages
[cognitive_resource_portal]: https://docs.microsoft.com/azure/cognitive-services/cognitive-services-apis-create-account
[cognitive_resource_cli]: https://docs.microsoft.com/azure/cognitive-services/cognitive-services-apis-create-account-cli

[language_detection]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/how-tos/text-analytics-how-to-language-detection
[sentiment_analysis]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/how-tos/text-analytics-how-to-sentiment-analysis
[key_phrase_extraction]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/how-tos/text-analytics-how-to-keyword-extraction
[named_entity_recognition]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/how-tos/text-analytics-how-to-entity-linking
[named_entities_categories]: https://docs.microsoft.com/azure/cognitive-services/Text-Analytics/named-entity-types

[textanalytics_client_class]: src/TextAnalyticsClient.cs
[azure_identity]: ../../identity/Azure.Identity
[cognitive_auth]: https://docs.microsoft.com/azure/cognitive-services/authentication
[register_aad_app]: https://docs.microsoft.com/azure/cognitive-services/authentication#assign-a-role-to-a-service-principal
[aad_grant_access]: https://docs.microsoft.com/azure/cognitive-services/authentication#assign-a-role-to-a-service-principal
[custom_subdomain]: https://docs.microsoft.com/azure/cognitive-services/authentication#create-a-resource-with-a-custom-subdomain
[DefaultAzureCredential]: ../../identity/Azure.Identity/README.md
[logging]: ../../core/Azure.Core/samples/Diagnostics.md
[data_limits]: https://docs.microsoft.com/azure/cognitive-services/text-analytics/overview#data-limits

[detect_language_sample0]: tests/samples/Sample1_DetectLanguage.cs
[detect_language_sample1]: tests/samples/Sample1_DetectLanguageBatchConvenience.cs
[detect_language_sample2]: tests/samples/Sample1_DetectLanguageBatch.cs
[detect_language_sample_async]: tests/samples/Sample1_DetectLanguageAsync.cs
[analyze_sentiment_sample0]: tests/samples/Sample2_AnalyzeSentiment.cs
[analyze_sentiment_sample1]: tests/samples/Sample2_AnalyzeSentimentBatchConvenience.cs
[analyze_sentiment_sample2]: tests/samples/Sample2_AnalyzeSentimentBatch.cs
[extract_key_phrases_sample0]: tests/samples/Sample3_ExtractKeyPhrases.cs
[extract_key_phrases_sample1]: tests/samples/Sample3_ExtractKeyPhrasesBatchConvenience.cs
[extract_key_phrases_sample2]: tests/samples/Sample3_ExtractKeyPhrasesBatch.cs
[recognize_entities_sample0]: tests/samples/Sample4_RecognizeEntities.cs
[recognize_entities_sample1]: tests/samples/Sample4_RecognizeEntitiesBatchConvenience.cs
[recognize_entities_sample2]: tests/samples/Sample4_RecognizeEntitiesBatch.cs
[recognize_entities_sample_async]: tests/samples/Sample4_RecognizeEntitiesAsync.cs
[recognize_pii_entities_sample0]: tests/samples/Sample5_RecognizePiiEntities.cs
[recognize_pii_entities_sample1]: tests/samples/Sample5_RecognizePiiEntitiesBatch.cs
[recognize_pii_entities_sample2]: tests/samples/Sample5_RecognizePiiEntitiesBatchConvenience.cs
[recognize_linked_entities_sample0]: tests/samples/Sample6_RecognizeLinkedEntities.cs
[recognize_linked_entities_sample1]: tests/samples/Sample6_RecognizeLinkedEntitiesBatch.cs
[recognize_linked_entities_sample2]: tests/samples/Sample6_RecognizeLinkedEntitiesBatchConvenience.cs

[azure_cli]: https://docs.microsoft.com/cli/azure
[azure_sub]: https://azure.microsoft.com/free/
[nuget]: https://www.nuget.org/
[azure_portal]: https://portal.azure.com

[cla]: https://cla.microsoft.com
[code_of_conduct]: https://opensource.microsoft.com/codeofconduct/
[coc_faq]: https://opensource.microsoft.com/codeofconduct/faq/
[coc_contact]: mailto:opencode@microsoft.com
