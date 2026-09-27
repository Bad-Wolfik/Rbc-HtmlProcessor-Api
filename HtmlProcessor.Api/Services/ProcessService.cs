using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using Dapper;
using FluentValidation;
using HtmlProcessor.Api.Models;
using Npgsql;

namespace HtmlProcessor.Api.Services;

public class ProcessService : IProcessService
{
    private readonly IValidator<ProcessRequest> _validator;
    private readonly string _connectionString;
    private readonly Regex _emailRegex;

    public ProcessService(
        IValidator<ProcessRequest> validator,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _validator = validator;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");

        _emailRegex = new Regex(@"[\w\.-]+@[\w\.-]+\.[a-z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public async Task<ProcessResponse> ProcessAsync(ProcessRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var failure = validationResult.Errors[0];
            return new ProcessResponse
            {
                IsError = 1,
                ErrorCode = failure.ErrorCode,
                ErrorMessage = failure.ErrorMessage
            };
        }

        try
        {
            string url = Encoding.UTF8.GetString(Convert.FromBase64String(request.UrlB64!));
            string htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(request.PageB64!));

            var browsingConfig = Configuration.Default;
            var context = BrowsingContext.New(browsingConfig);

            using var document = await context.OpenAsync(req => req.Content(htmlContent), cancellationToken);

            var foundElements = document.QuerySelectorAll(request.Selector!).ToList();
            var attributesList = new List<string>(foundElements.Count);
            var dbEntities = new List<ElementEntity>(foundElements.Count);

            foreach (var element in foundElements)
            {
                var attrValue = element.GetAttribute(request.Attribute!) ?? string.Empty;
                attributesList.Add(attrValue);

                dbEntities.Add(new ElementEntity
                {
                    AttributeValue = attrValue,
                    HtmlContent = element.OuterHtml
                });
            }

            if (dbEntities.Count > 0)
            {
                const string insertSql = """
                    INSERT INTO elements (attribute_value, html_content)
                    VALUES (@AttributeValue, @HtmlContent);
                    """;

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync(new CommandDefinition(
                    insertSql,
                    dbEntities,
                    cancellationToken: cancellationToken));
            }

            var emailMatches = _emailRegex.Matches(htmlContent);
            var emailsList = new List<string>(emailMatches.Count);

            foreach (Match match in emailMatches)
            {
                emailsList.Add(match.Value);
            }

            byte[] keyBytes = Convert.FromBase64String(request.KeyBytesB64!);
            byte[] cipherBytes = Convert.FromBase64String(request.EncryptedTextBytesB64!);
            string decryptedText = DecryptAes256Ecb(cipherBytes, keyBytes);

            return new ProcessResponse
            {
                IsError = 0,
                ErrorCode = null,
                ErrorMessage = null,
                ElementsCount = foundElements.Count,
                EmailsCount = emailsList.Count,
                Url = url,
                DecryptedPlainText = decryptedText,
                ElementsAttrList = attributesList,
                EmailsList = emailsList
            };
        }
        catch (Exception ex)
        {
            return new ProcessResponse
            {
                IsError = 1,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    private static string DecryptAes256Ecb(byte[] cipherBytes, byte[] keyBytes)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, new byte[16]);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }
        catch
        {
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            using var decryptor = aes.CreateDecryptor(aes.Key, new byte[16]);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }
    }
}