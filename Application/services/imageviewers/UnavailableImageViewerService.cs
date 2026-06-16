namespace Application.services.imageviewers;

public class UnavailableImageViewerService : IImageViewerService
{
        private readonly string _reason;

        public UnavailableImageViewerService(string reason)
        {
            _reason = reason;
        }

        public bool IsAvailable()
        {
            return false;
        }

        public string GetImageViewerName()
        {
            return "Unavailable";
        }
        
        public void OpenImage(string imagePath)
        {
            throw new InvalidOperationException(_reason);
        }

        public string? GetOpenedFileName(string? givenFileName)
        {
            if (givenFileName is not null)
            {
                return givenFileName;
            }

            throw new InvalidOperationException(_reason);
        }
    }   