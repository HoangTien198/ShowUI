using System.Data;
using System.Windows.Forms;

namespace ShowUIApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShowUI.SFISB05_SV.Servicepostdata objSfisSv = new ShowUI.SFISB05_SV.Servicepostdata();

            DataTable dtCurrentStationTestedDUT = objSfisSv.GET_TOTAL_PASSFAIL("FT", "D", "2020102808", "2020102819", "");
            var a = dtCurrentStationTestedDUT;
        }
    }
}