using System.ComponentModel;
using System.Windows.Forms;

using IdSharp.AudioInfo;
using IdSharp.AudioInfo.Mpeg.Inspection;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;
using IdSharp.Tagging.ID3v2.Frames;

using SharpImage = SixLabors.ImageSharp.Image;

namespace IdSharp.Tagging.Harness.WinForms.UserControls;

public partial class ID3v2UserControl : UserControl
{
    private IID3v2Tag _id3v2;

    public ID3v2UserControl()
    {
        InitializeComponent();

        cmbGenre.Sorted = true;
        cmbGenre.Items.AddRange(GenreHelper.GenreByIndex);

        cmbImageType.Items.AddRange(PictureTypeHelper.PictureTypeStrings);
    }

    private void bindingSource_CurrentChanged(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        if (attachedPicture != null)
        {
            LoadImageData(attachedPicture);
        }
        else
        {
            ClearImageData();
        }
    }

    private void imageContextMenu_Opening(object sender, CancelEventArgs e)
    {
        miSaveImage.Enabled = (this.pictureBox1.Image != null);
        miLoadImage.Enabled = (GetCurrentPictureFrame() != null);
    }

    private void miSaveImage_Click(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        SaveImageToFile(attachedPicture);
    }

    private void miLoadImage_Click(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        LoadImageFromFile(attachedPicture);
    }

    private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        LoadImageFromFile(attachedPicture);
    }

    private void cmbImageType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        if (attachedPicture != null)
        {
            attachedPicture.PictureType = PictureTypeHelper.GetPictureTypeFromString(cmbImageType.Text);
        }
    }

    private void txtImageDescription_Validated(object sender, EventArgs e)
    {
        var attachedPicture = GetCurrentPictureFrame();
        if (attachedPicture != null)
        {
            attachedPicture.Description = txtImageDescription.Text;
        }
    }

    private void LoadImageData(IAttachedPicture attachedPicture)
    {
        pictureBox1.Image = attachedPicture.Picture?.ToBitmap();

        txtImageDescription.Text = attachedPicture.Description;
        cmbImageType.SelectedIndex = cmbImageType.Items.IndexOf(PictureTypeHelper.GetStringFromPictureType(attachedPicture.PictureType));

        txtImageDescription.Enabled = true;
        cmbImageType.Enabled = true;
    }

    private void ClearImageData()
    {
        pictureBox1.Image = null;
        txtImageDescription.Text = "";
        cmbImageType.SelectedIndex = -1;

        txtImageDescription.Enabled = false;
        cmbImageType.Enabled = false;
    }

    private void SaveImageToFile(IAttachedPicture attachedPicture)
    {
        var extension = attachedPicture.PictureExtension;

        imageSaveFileDialog.FileName = "image." + extension;

        var dialogResult = imageSaveFileDialog.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            using (var fs = File.Open(imageSaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(attachedPicture.PictureData, 0, attachedPicture.PictureData.Length);
            }
        }
    }

    private void LoadImageFromFile(IAttachedPicture attachedPicture)
    {
        var dialogResult = imageOpenFileDialog.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
            attachedPicture.Picture = SharpImage.Load(imageOpenFileDialog.FileName);
            pictureBox1.Image = attachedPicture.Picture.ToBitmap();
        }
    }

    private IAttachedPicture GetCurrentPictureFrame()
    {
        if (imageBindingNavigator.BindingSource == null)
        {
            return null;
        }

        return imageBindingNavigator.BindingSource.Current as IAttachedPicture;
    }

    public void LoadFile(string path)
    {
        ClearImageData();

        _id3v2 = new ID3v2Tag(path);

        txtFilename.Text = Path.GetFileName(path);
        txtArtist.Text = _id3v2.Artist;
        txtTitle.Text = _id3v2.Title;
        txtAlbum.Text = _id3v2.Album;
        cmbGenre.Text = _id3v2.Genre;
        txtYear.Text = _id3v2.Year;
        txtTrackNumber.Text = _id3v2.TrackNumber;
        chkPodcast.Checked = _id3v2.IsPodcast;
        txtPodcastFeedUrl.Text = _id3v2.PodcastFeedUrl;

        var bindingSource = new BindingSource();
        imageBindingNavigator.BindingSource = bindingSource;
        bindingSource.CurrentChanged += bindingSource_CurrentChanged;
        bindingSource.DataSource = _id3v2.PictureList;

        switch (_id3v2.Header.TagVersion)
        {
            case ID3v2TagVersion.ID3v22:
                cmbID3v2.SelectedIndex = cmbID3v2.Items.IndexOf("ID3v2.2");
                break;
            case ID3v2TagVersion.ID3v23:
                cmbID3v2.SelectedIndex = cmbID3v2.Items.IndexOf("ID3v2.3");
                break;
            case ID3v2TagVersion.ID3v24:
                cmbID3v2.SelectedIndex = cmbID3v2.Items.IndexOf("ID3v2.4");
                break;
        }

        txtPlayLength.Text = string.Empty;
        txtBitrate.Text = string.Empty;
        txtEncoderPreset.Text = string.Empty;

        _ = Task.Run(() => LoadAudioFileDetails(path));
    }

    private void LoadAudioFileDetails(string path)
    {
        var audioFile = AudioFile.Create(path, false);
        _ = audioFile.Bitrate; // force bitrate calculation

        var lameTagReader = new DescriptiveLameTagReader(path);

        Invoke(() => SetAudioFileDetails(audioFile, lameTagReader));
    }

    private void SetAudioFileDetails(IAudioFile audioFile, DescriptiveLameTagReader lameTagReader)
    {
        txtPlayLength.Text = $"{(int)audioFile.TotalSeconds / 60}:{(int)audioFile.TotalSeconds % 60:00}";
        txtBitrate.Text = $"{audioFile.Bitrate:#,0} kbps";
        txtEncoderPreset.Text = $"{lameTagReader.LameTagInfoEncoder} {(lameTagReader.UsePresetGuess == UsePresetGuess.NotNeeded ? lameTagReader.Preset : lameTagReader.PresetGuess)}";
    }

    public void SaveFile(string path)
    {
        if (_id3v2 == null)
        {
            MessageBox.Show("Nothing to save!");
            return;
        }

        if (cmbID3v2.SelectedIndex == cmbID3v2.Items.IndexOf("ID3v2.2"))
        {
            _id3v2.Header.TagVersion = ID3v2TagVersion.ID3v22;
        }
        else if (cmbID3v2.SelectedIndex == cmbID3v2.Items.IndexOf("ID3v2.3"))
        {
            _id3v2.Header.TagVersion = ID3v2TagVersion.ID3v23;
        }
        else if (cmbID3v2.SelectedIndex == cmbID3v2.Items.IndexOf("ID3v2.4"))
        {
            _id3v2.Header.TagVersion = ID3v2TagVersion.ID3v24;
        }
        else
        {
            throw new Exception("Unknown tag version");
        }

        _id3v2.Artist = txtArtist.Text;
        _id3v2.Title = txtTitle.Text;
        _id3v2.Album = txtAlbum.Text;
        _id3v2.Genre = cmbGenre.Text;
        _id3v2.Year = txtYear.Text;
        _id3v2.TrackNumber = txtTrackNumber.Text;
        _id3v2.IsPodcast = chkPodcast.Checked;
        _id3v2.PodcastFeedUrl = txtPodcastFeedUrl.Text;
        _id3v2.Save(path);
    }

    private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
    {
        Console.WriteLine(_id3v2.PictureList.Count);
    }

}
