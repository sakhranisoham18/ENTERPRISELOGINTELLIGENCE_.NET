document.addEventListener('DOMContentLoaded',()=>{
  // Generator presets fill the count field.
  document.querySelectorAll('.preset-row button').forEach(btn=>btn.addEventListener('click',()=>{
    const input=document.querySelector('input[name="count"]');
    if(input)input.value=btn.dataset.value;
  }));

  // Stagger the first rows of each table so a page "prints" in once on load.
  document.querySelectorAll('.content tbody').forEach(body=>{
    Array.from(body.rows).slice(0,12).forEach((row,i)=>row.style.setProperty('--i',i));
  });
});
