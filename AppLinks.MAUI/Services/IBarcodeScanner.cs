
namespace AppLinks.MAUI
{
    public interface IBarcodeScanner
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IBarcodeScanner"/>.
        /// </summary>
        public static IBarcodeScanner Current { get; set; } = BarcodeScanner.Current;

    }
}