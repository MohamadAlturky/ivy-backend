namespace IvyBackend.Services;

public interface IMessageStore
{
    /// <summary>
    /// Gets a localized message by code and language
    /// </summary>
    /// <param name="messageCode">The message code</param>
    /// <param name="language">The language code (e.g., "en", "ar")</param>
    /// <returns>The localized message or the message code if not found</returns>
    string GetMessage(string messageCode, string language = "en");
}