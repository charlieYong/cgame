//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Option: missing-value detection (*Specified/ShouldSerialize*/Reset*) enabled
    
// Generated from: SynAttr.proto
namespace SLMS
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"csSynAttr")]
  public partial class csSynAttr : global::ProtoBuf.IExtensible
  {
    public csSynAttr() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"scSynAttr")]
  public partial class scSynAttr : global::ProtoBuf.IExtensible
  {
    public scSynAttr() {}
    
    private ulong? _guid;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"guid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public ulong guid
    {
      get { return _guid?? default(ulong); }
      set { _guid = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool guidSpecified
    {
      get { return this._guid != null; }
      set { if (value == (this._guid== null)) this._guid = value ? this.guid : (ulong?)null; }
    }
    private bool ShouldSerializeguid() { return guidSpecified; }
    private void Resetguid() { guidSpecified = false; }
    
    private string _name;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string name
    {
      get { return _name?? ""; }
      set { _name = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool nameSpecified
    {
      get { return this._name != null; }
      set { if (value == (this._name== null)) this._name = value ? this.name : (string)null; }
    }
    private bool ShouldSerializename() { return nameSpecified; }
    private void Resetname() { nameSpecified = false; }
    
    private string _sign;
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"sign", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string sign
    {
      get { return _sign?? ""; }
      set { _sign = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool signSpecified
    {
      get { return this._sign != null; }
      set { if (value == (this._sign== null)) this._sign = value ? this.sign : (string)null; }
    }
    private bool ShouldSerializesign() { return signSpecified; }
    private void Resetsign() { signSpecified = false; }
    
    private string _icon;
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"icon", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string icon
    {
      get { return _icon?? ""; }
      set { _icon = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool iconSpecified
    {
      get { return this._icon != null; }
      set { if (value == (this._icon== null)) this._icon = value ? this.icon : (string)null; }
    }
    private bool ShouldSerializeicon() { return iconSpecified; }
    private void Reseticon() { iconSpecified = false; }
    
    private uint? _sex;
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"sex", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint sex
    {
      get { return _sex?? default(uint); }
      set { _sex = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool sexSpecified
    {
      get { return this._sex != null; }
      set { if (value == (this._sex== null)) this._sex = value ? this.sex : (uint?)null; }
    }
    private bool ShouldSerializesex() { return sexSpecified; }
    private void Resetsex() { sexSpecified = false; }
    
    private uint? _lvl;
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"lvl", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint lvl
    {
      get { return _lvl?? default(uint); }
      set { _lvl = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool lvlSpecified
    {
      get { return this._lvl != null; }
      set { if (value == (this._lvl== null)) this._lvl = value ? this.lvl : (uint?)null; }
    }
    private bool ShouldSerializelvl() { return lvlSpecified; }
    private void Resetlvl() { lvlSpecified = false; }
    
    private uint? _exp;
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"exp", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint exp
    {
      get { return _exp?? default(uint); }
      set { _exp = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool expSpecified
    {
      get { return this._exp != null; }
      set { if (value == (this._exp== null)) this._exp = value ? this.exp : (uint?)null; }
    }
    private bool ShouldSerializeexp() { return expSpecified; }
    private void Resetexp() { expSpecified = false; }
    
    private uint? _diamond;
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"diamond", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint diamond
    {
      get { return _diamond?? default(uint); }
      set { _diamond = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool diamondSpecified
    {
      get { return this._diamond != null; }
      set { if (value == (this._diamond== null)) this._diamond = value ? this.diamond : (uint?)null; }
    }
    private bool ShouldSerializediamond() { return diamondSpecified; }
    private void Resetdiamond() { diamondSpecified = false; }
    
    private uint? _money;
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"money", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public uint money
    {
      get { return _money?? default(uint); }
      set { _money = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool moneySpecified
    {
      get { return this._money != null; }
      set { if (value == (this._money== null)) this._money = value ? this.money : (uint?)null; }
    }
    private bool ShouldSerializemoney() { return moneySpecified; }
    private void Resetmoney() { moneySpecified = false; }
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"AttrType")]
    public enum AttrType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_GUID", Value=0)]
      AT_GUID = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_NAME", Value=1)]
      AT_NAME = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_SIGN", Value=2)]
      AT_SIGN = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_ICON", Value=3)]
      AT_ICON = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_SEX", Value=4)]
      AT_SEX = 4,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_LVL", Value=5)]
      AT_LVL = 5,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_EXP", Value=6)]
      AT_EXP = 6,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_GOLD", Value=7)]
      AT_GOLD = 7,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_DIAMOND", Value=8)]
      AT_DIAMOND = 8,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_ENERGY", Value=9)]
      AT_ENERGY = 9,
            
      [global::ProtoBuf.ProtoEnum(Name=@"AT_SYN_ATTR_MAX", Value=32)]
      AT_SYN_ATTR_MAX = 32
    }
  
}