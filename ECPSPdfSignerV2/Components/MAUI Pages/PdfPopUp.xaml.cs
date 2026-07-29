using CommunityToolkit.Maui.Views;

namespace ECPSPdfSignerV2.Components.MAUI_Pages; 

public partial class PdfPopUp : Popup
{
	public PdfPopUp(string localFilePath, string backBtnText, string closeBtnText, bool singleButton)
	{
		InitializeComponent();
		pdfview.Source = localFilePath;

		var text = Path.GetFileNameWithoutExtension(localFilePath);
		if (text.Contains('_'))
		{
			text = text.Replace("_", "");
		}

        headerLabel.Text = $"File No:  " + text;
        CloseButton.Text = closeBtnText;
		BackButton.Text = backBtnText;	

		if(singleButton)
		{
			BackButton.IsVisible = false;
		}
		else
		{
			BackButton.IsVisible = true;	
		}

		// Disable 'Proceed to Sign' button when the file is corrupted
        var fileContent = File.ReadAllBytes(localFilePath);

		if (((!File.Exists(localFilePath) || fileContent == null || fileContent.Length == 0)) && !singleButton)
		{
			CloseButton.IsEnabled = false;	
		}
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close(true);
    }
}