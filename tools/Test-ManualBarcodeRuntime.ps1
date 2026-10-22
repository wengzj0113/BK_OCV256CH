$ErrorActionPreference = 'Stop'
$exe = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\build-verify-manual-barcode\XL_OCV_256CH.exe' }
$exe = (Resolve-Path $exe).Path
$assembly = [Reflection.Assembly]::LoadFrom($exe)
if ($null -eq $assembly.GetType('software.ManualTestBarcodePlan', $false)) { throw 'ManualTestBarcodePlan is not implemented.' }
$source = @'
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using software;
public sealed class UnavailableMes : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken) {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){Content=new StringContent("unavailable")});
    }
}
public static class ManualBarcodeChecks {
    static int checks;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    static DataTable Cells() { var t=new DataTable(); t.Columns.Add("ChannelNo",typeof(int)); t.Columns.Add("BatteryCode",typeof(string)); return t; }
    static void Reject(Action action,string message) { bool caught=false; try { action(); } catch(ArgumentException) { caught=true; } catch(InvalidOperationException) { caught=true; } Check(caught,message); }
    static Type batType=typeof(ManualTestRoundSnapshot).Assembly.GetType("software.Module1+Bat_info");
    static void Set(Array data,int row,string name,object value) { var item=data.GetValue(row); batType.GetField(name).SetValue(item,value); data.SetValue(item,row); }
    static object Get(Array data,int row,string name) { return batType.GetField(name).GetValue(data.GetValue(row)); }
    static ManualTestRoundSnapshot Capture(ManualTestRunContext c,Array a,DateTime s,DateTime e) { return (ManualTestRoundSnapshot)typeof(ManualTestRoundSnapshot).GetMethod("Capture",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{c,1,a,s,e}); }
    static Array Copy(ManualTestRoundSnapshot round) { return (Array)typeof(ManualTestRoundSnapshot).GetMethod("CopyMeasurements",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Invoke(round,null); }
    public static string Run() {
        var date=new DateTime(2026,10,19,12,0,0);
        var empty=Cells(); var p=ManualTestBarcodePlan.Create(" TP001 ",empty,date);
        Check(p.IsSimulated && p.Codes.Count==256 && p.TrayCode=="TP001","Empty tray must create a complete simulated map.");
        Check(p.Codes[1]==ManualTestBarcodePlan.Create("TP001",empty,date).Codes[1],"Repeat preparation must have stable codes.");
        Check(p.Codes[1]!=ManualTestBarcodePlan.Create("TP002",empty,date).Codes[1],"Different trays must not share simulated codes.");
        Check(p.Codes.Values.Distinct().Count()==256 && p.Codes.Values.All(x=>x.StartsWith("TEST-") && x.Length<=100),"Generated codes must be distinct and bounded.");
        var table=Cells(); table.Rows.Add(1,"00001234"); table.Rows.Add(256,"B256");
        var real=ManualTestBarcodePlan.Create("TP001",table,date);
        Check(!real.IsSimulated && real.Codes.Count==2 && real.Codes[1]=="00001234","Partial real map must remain real and retain leading zeros.");
        Reject(()=>real.ValidateChannels(new[]{1,2}),"Missing selected channel must fail.");
        Reject(()=>real.ValidateChannels(new int[0]),"Empty selection must fail.");
        table.Rows.Add(2,"00001234"); Reject(()=>ManualTestBarcodePlan.Create("T",table,date),"Repeated battery code must fail.");
        table=Cells(); table.Rows.Add(1,"A"); table.Rows.Add(1,"B"); Reject(()=>ManualTestBarcodePlan.Create("T",table,date),"Repeated channel must fail.");
        table=Cells(); table.Rows.Add(257,"A"); Reject(()=>ManualTestBarcodePlan.Create("T",table,date),"Invalid channel must fail.");
        Reject(()=>ManualTestBarcodePlan.Create(" ",empty,date),"Empty tray must fail.");
        var ctx=new ManualTestRunContext(real,new[]{256,1},2,"operator-1",date,"process-1","25.0","99",true,"http://127.0.0.1:19015","id","password",3000,4000,0,20);
        var data=Array.CreateInstance(batType,256);
        Set(data,0,"Voltage",3.65f); Set(data,0,"Resistance",1.2f); Set(data,0,"RawResistanceMilliohm",1.1d); Set(data,0,"CalibrationValueMilliohm",.1d); Set(data,0,"FinalResistanceMilliohm",1.2d);
        Set(data,255,"Voltage",2.0f); Set(data,255,"Resistance",2.0f); Set(data,255,"FinalResistanceMilliohm",2.0d);
        var round=Capture(ctx,data,date,date.AddSeconds(2));
        Set(data,0,"Voltage",99f); Set(data,0,"BatCode","overwritten");
        var cloned=Copy(round); Set(cloned,0,"Voltage",77f);
        Check((float)Get(Copy(round),0,"Voltage")<4 && (string)Get(Copy(round),0,"BatCode")=="00001234","Snapshot must fix barcode and isolate original/returned arrays.");
        var inputs=round.CreateResultInputs();
        Check(inputs.Count==2 && !inputs[0].IsNg && inputs[1].IsNg && inputs[0].CalibrationValueMilliohm==.1m,"Persisted snapshot must retain calibration and captured verdict.");
        Check(ctx.OcvStageCode=="OCV1" && ctx.Channels.SequenceEqual(new[]{1,256}),"Context must capture stage and sorted sparse channels.");
        var uploader=new ResultDataUploadAssembly(null,round);
        var json=uploader.BuildPayloadJson();
        Check(json.Contains("00001234") && json.Contains("B256") && json.Contains("TP001") && json.Contains("operator-1") && json.Contains("process-1"),"MES payload must use explicit captured context.");
        Check(json.Contains("\"Is_NG\":1") && json.Contains("3.65000"),"MES must contain captured measurements and NG verdict.");
        var module=typeof(ManualTestBarcodePlan).Assembly.GetType("software.Module1");
        var settingsField=module.GetField("Initsetting",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
        var savedSettings=settingsField.GetValue(null);
        var failingSettings=settingsField.GetValue(null);
        failingSettings.GetType().GetField("strDatabase").SetValue(failingSettings,"127.0.0.1,1");
        settingsField.SetValue(null,failingSettings);
        try {
            using(var http=new HttpClient(new UnavailableMes())) {
                typeof(ResultDataUploadAssembly).GetField("client",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(uploader,http);
                bool queueFailed=false; try { uploader.UploadSnapshotAsync().GetAwaiter().GetResult(); } catch { queueFailed=true; }
                Check(queueFailed,"Manual upload must report outbox write failure to its awaited caller.");
                var callback=(Task)typeof(ResultDataUploadAssembly).GetMethod("TimerCallback",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(uploader,null);
                callback.GetAwaiter().GetResult();
                Check(callback.Status==TaskStatus.RanToCompletion,"Legacy async timer boundary must contain upload/outbox exceptions.");
            }
        } finally { settingsField.SetValue(null,savedSettings); }
        var debugContext=new ManualTestRunContext(p,new[]{1},1,"operator",date);
        var debugRound=Capture(debugContext,Array.CreateInstance(batType,256),date,date);
        Reject(()=>new ResultDataUploadAssembly(null,debugRound),"Simulated results must not enter uploader.");
        using(var form=new FrmManualTest()) {
            form.StartPosition=System.Windows.Forms.FormStartPosition.Manual;
            form.Location=new System.Drawing.Point(-30000,-30000);
            form.ShowInTaskbar=false; form.Opacity=0; form.Show();
            form.CreateControl(); form.PerformLayout();
            Check(form.Controls.Find("txtTrayCode",true).Length==1 && form.Controls.Find("btnGetBarcode",true).Length==1,"Designer barcode controls must be wired and uniquely named.");
            var grid=(System.Windows.Forms.DataGridView)form.Controls.Find("resultGrid",true)[0];
            Check(grid.Columns.Count==6 && grid.Columns.Contains("BatteryCode"),"Result table must include barcode column.");
            Check(form.Controls.Find("Button1",true).Length==0,"Unused placeholder button must be removed.");
            foreach(var width in new[]{1459,1209}) {
                form.ClientSize=new System.Drawing.Size(width,936); form.PerformLayout();
                var toolbar=(System.Windows.Forms.FlowLayoutPanel)form.Controls.Find("commandPanel",true)[0]; toolbar.PerformLayout();
                foreach(System.Windows.Forms.Control control in toolbar.Controls) Check(control.Bottom<=toolbar.ClientSize.Height-toolbar.Padding.Bottom,"Toolbar item outside visible area: "+control.Name+" at width "+width);
            }
            form.ClientSize=new System.Drawing.Size(1459,936); form.PerformLayout();
            using(var bitmap=new System.Drawing.Bitmap(form.Width,form.Height)) {
                form.DrawToBitmap(bitmap,new System.Drawing.Rectangle(System.Drawing.Point.Empty,form.Size));
                bitmap.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(FrmManualTest).Assembly.Location),"manual-form-preview.png"));
            }
        }
        return "Manual barcode runtime: PASS ("+checks+" assertions)";
    }
}
'@
$compiler = New-Object System.CodeDom.Compiler.CompilerParameters
$compiler.GenerateInMemory = $true
foreach ($reference in @($exe,'System.dll','System.Data.dll','System.Core.dll','System.Xml.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Net.Http.dll')) { [void]$compiler.ReferencedAssemblies.Add($reference) }
Add-Type -TypeDefinition $source -CompilerParameters $compiler
[ManualBarcodeChecks]::Run()
