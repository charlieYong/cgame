//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Option: missing-value detection (*Specified/ShouldSerialize*/Reset*) enabled
    
// Generated from: NotifyTip.proto
// Note: requires additional types generated from: ResultCode.proto
namespace SLMS
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"csNotifyTip")]
  public partial class csNotifyTip : global::ProtoBuf.IExtensible
  {
    public csNotifyTip() {}
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"scNotifyTip")]
  public partial class scNotifyTip : global::ProtoBuf.IExtensible
  {
    public scNotifyTip() {}
    
    private SLMS.ResultCode _ret;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"ret", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public SLMS.ResultCode ret
    {
      get { return _ret; }
      set { _ret = value; }
    }
    private string _para;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"para", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string para
    {
      get { return _para?? ""; }
      set { _para = value; }
    }
    [global::System.Xml.Serialization.XmlIgnore]
    [global::System.ComponentModel.Browsable(false)]
    public bool paraSpecified
    {
      get { return this._para != null; }
      set { if (value == (this._para== null)) this._para = value ? this.para : (string)null; }
    }
    private bool ShouldSerializepara() { return paraSpecified; }
    private void Resetpara() { paraSpecified = false; }
    
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}