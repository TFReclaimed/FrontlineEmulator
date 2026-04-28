namespace Frontline.Extensions;

public static class StringExtensions
{
    public static string[] SplitForChat(this string text, int chunkSize)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        if (text.Length <= chunkSize)
        {
            return [text];
        }

        if (chunkSize < 7)
        {
            throw new ArgumentException("Chunk size must be at least 7 to accommodate the ellipsis.");
        }

        var chunks = new List<string>();
        var currentIndex = 0;

        while (currentIndex < text.Length)
        {
            var isFirst = currentIndex == 0;
            var remaining = text.Length - currentIndex;

            var dotsAtStart = isFirst ? 0 : 3;
            var availableCapacity = chunkSize - dotsAtStart - 3;

            if (remaining <= chunkSize - dotsAtStart)
            {
                var lastPart = text[currentIndex..];
                chunks.Add((isFirst ? "" : "...") + lastPart);
                break;
            }

            var segment = text.Substring(currentIndex, availableCapacity);
            var formattedChunk = (isFirst ? "" : "...") + segment + "...";
            chunks.Add(formattedChunk);

            currentIndex += availableCapacity;
        }

        return chunks.ToArray();
    }
}