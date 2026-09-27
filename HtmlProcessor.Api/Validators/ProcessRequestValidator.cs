using FluentValidation;
using HtmlProcessor.Api.Models;

namespace HtmlProcessor.Api.Validators;

public class ProcessRequestValidator : AbstractValidator<ProcessRequest>
{
    public ProcessRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Selector)
            .NotEmpty()
            .WithErrorCode("EMPTY_SELECTOR")
            .WithMessage("Пустой селектор во входящем объекте.");

        RuleFor(x => x.Attribute)
            .NotEmpty()
            .WithErrorCode("EMPTY_ATTRIBUTE")
            .WithMessage("Пустой атрибут во входящем объекте.");

        RuleFor(x => x.UrlB64)
            .NotEmpty()
            .WithErrorCode("MISSING_URL")
            .WithMessage("Отсутствует параметр url_b64 во входящем объекте.")
            .Must(IsValidBase64)
            .WithErrorCode("INVALID_URL_BASE64")
            .WithMessage("Ошибка при декодинге base64 в URL.");

        RuleFor(x => x.PageB64)
            .NotEmpty()
            .WithErrorCode("MISSING_PAGE")
            .WithMessage("Отсутствует параметр page_b64 во входящем объекте.")
            .Must(IsValidBase64)
            .WithErrorCode("INVALID_PAGE_BASE64")
            .WithMessage("Ошибка при декодинге base64 в Page.");

        RuleFor(x => x.KeyBytesB64)
            .NotEmpty()
            .WithErrorCode("MISSING_KEY")
            .WithMessage("Отсутствует параметр key_bytes_b64 во входящем объекте.")
            .Must(IsValidBase64)
            .WithErrorCode("INVALID_KEY_BASE64")
            .WithMessage("Ошибка при декодинге base64 ключа шифрования.");

        RuleFor(x => x.EncryptedTextBytesB64)
            .NotEmpty()
            .WithErrorCode("MISSING_ENCRYPTED_TEXT")
            .WithMessage("Отсутствует параметр encrypted_text_bytes_b64 во входящем объекте.")
            .Must(IsValidBase64)
            .WithErrorCode("INVALID_CIPHERTEXT_BASE64")
            .WithMessage("Ошибка при декодинге base64 шифротекста.");
    }

    private bool IsValidBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        const int Base64BlockCharCount = 4;
        const int DecodedBlockByteCount = 3;
        const int CeilingRoundingOffset = Base64BlockCharCount - 1;

        int estimatedByteCount = (value.Length * DecodedBlockByteCount + CeilingRoundingOffset) / Base64BlockCharCount;
        Span<byte> buffer = new byte[estimatedByteCount];

        int writtenBytesCount;
        return Convert.TryFromBase64String(value, buffer, out writtenBytesCount);
    }
}