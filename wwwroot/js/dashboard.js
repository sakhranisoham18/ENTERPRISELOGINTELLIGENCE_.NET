(function(){
  const d=window.dashboardData||{};
  const labels=x=>(x||[]).map(a=>a.label);
  const values=x=>(x||[]).map(a=>a.value);
  const INK='#16241D', INK2='#4B5C51', HAIR='#C5D0B6', BLUE='#2437A3';

  if(window.Chart){
    Chart.defaults.font.family="'Martian Mono', ui-monospace, monospace";
    Chart.defaults.font.size=11;
    Chart.defaults.color=INK2;
    Chart.defaults.animation=false;

    const tooltip={backgroundColor:INK,titleColor:'#F4F6EA',bodyColor:'#F4F6EA',cornerRadius:2,padding:9,displayColors:false};
    const grid={color:HAIR,lineWidth:1};
    const axisLine={color:INK,width:1.5};

    const vol=document.getElementById('volumeChart');
    if(vol) new Chart(vol,{
      type:'bar',
      data:{labels:labels(d.volume),datasets:[{label:'Events',data:values(d.volume),backgroundColor:BLUE,hoverBackgroundColor:INK,borderRadius:0,categoryPercentage:.92,barPercentage:.86}]},
      options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false},tooltip},
        scales:{x:{grid:{display:false},border:axisLine,ticks:{maxRotation:0,autoSkipPadding:14}},
                y:{beginAtZero:true,grid,border:{display:false},ticks:{precision:0}}}}
    });

    const svc=document.getElementById('serviceChart');
    if(svc) new Chart(svc,{
      type:'bar',
      data:{labels:labels(d.services),datasets:[{label:'Events',data:values(d.services),backgroundColor:INK,hoverBackgroundColor:BLUE,borderRadius:0,barThickness:16}]},
      options:{indexAxis:'y',responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false},tooltip},
        scales:{x:{beginAtZero:true,grid,border:{display:false},ticks:{precision:0}},
                y:{grid:{display:false},border:axisLine,ticks:{color:INK,font:{family:"'Bricolage Grotesque', system-ui, sans-serif",size:13}}}}}
    });
  }

  if(window.signalR){
    const connection=new signalR.HubConnectionBuilder().withUrl('/hubs/dashboard').withAutomaticReconnect().build();
    connection.on('logsIngested',()=>{setTimeout(()=>location.reload(),400);});
    connection.start().catch(()=>{});
  }
})();
