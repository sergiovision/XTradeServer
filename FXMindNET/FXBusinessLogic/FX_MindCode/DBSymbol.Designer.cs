//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
namespace FXBusinessLogic.fx_mind {

	[Persistent(@"symbol")]
	public partial class DBSymbol : XPLiteObject {
		int fID;
		[Key]
		public int ID {
			get { return fID; }
			set { SetPropertyValue<int>("ID", ref fID, value); }
		}
		string fName;
		[Size(50)]
		public string Name {
			get { return fName; }
			set { SetPropertyValue<string>("Name", ref fName, value); }
		}
		string fDescription;
		[Size(500)]
		public string Description {
			get { return fDescription; }
			set { SetPropertyValue<string>("Description", ref fDescription, value); }
		}
		bool fDisabled;
		public bool Disabled {
			get { return fDisabled; }
			set { SetPropertyValue<bool>("Disabled", ref fDisabled, value); }
		}
		bool fUse4Tech;
		public bool Use4Tech {
			get { return fUse4Tech; }
			set { SetPropertyValue<bool>("Use4Tech", ref fUse4Tech, value); }
		}
		string fC1;
		[Size(10)]
		public string C1 {
			get { return fC1; }
			set { SetPropertyValue<string>("C1", ref fC1, value); }
		}
		string fC2;
		[Size(10)]
		public string C2 {
			get { return fC2; }
			set { SetPropertyValue<string>("C2", ref fC2, value); }
		}
	}

}
