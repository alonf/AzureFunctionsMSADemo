namespace UI
{
    public partial class FormTester : Form
    {
        private readonly ApiAccessor _apiAccessor = new ApiAccessor();

        public FormTester()
        {
            InitializeComponent();
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            var isSueedded = await _apiAccessor.SendAsync(textBoxName.Text, int.Parse(maskedTextBoxValue.Text));
            MessageBox.Show(isSueedded ? "Message was sent" : "Error sending message", "Result", MessageBoxButtons.OK, isSueedded ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}