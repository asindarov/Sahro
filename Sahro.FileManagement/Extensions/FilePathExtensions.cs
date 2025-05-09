using Sahro.FileManagement.Consts;

namespace Sahro.FileManagement.Extensions;

public static class FilePathExtensions
{
    public static string ContentType(this string fileName)
    {
        var fileExtension = fileName.Split('.')[^1];

        if (Enumerable.Contains(FileTypeContants.IMAGE_TYPES, fileExtension))
        {
            return $"image/{fileExtension}";
        }

        if (Enumerable.Contains(FileTypeContants.DOCUMENT_TYPES, fileExtension))
        {
            return $"application/{fileExtension}";
        }

        return "application/octet-stream";
    }
}