select b.codagencia,
    b.codtipocmpr,
    b.numcmprventa,
    b.numdocumento,
    pagos.codformapago,
    pagos.codtarjeta,
    pagos.codbanco,
    pagos.numtarjetacredito,
    pagos.cuotas_diferido,pagos.tipo_diferido,pagos.valor
    from faccmprventa b 
    inner join vw_emp_age_axe axe on(b.codagencia = axe.codagencia) 
    inner join cnttipocmpr co on(b.codtipocmpr = co.codtipocmpr) 
    left outer join vw_cliente_direcciones clidirec on (axe.codempresa = clidirec.codempresa and b.codcliente = clidirec.codcliente and b.coddireccion = clidirec.coddireccion) 
    left outer join gentiposclientes tipcli on (b.codtipocliente=tipcli.codtipocliente) 
    inner join vw_emp_cli_cxe_tcl_ven cli on(axe.codempresa = cli.codempresacli and b.codcliente = cli.codcliente) 
    inner join genvendedores vend on(b.codempresavend = vend.codempresa and b.codvendedor = vend.codvendedor) 
    inner join gentipovendedor tvend on(vend.codtipovendedor = tvend.codtipovendedor) 
    left outer join GENTIPOSNOTACREDITO tnc on (b.tiponc = tnc.tiponc) 
    inner join genestados estfac on(b.codestado = estfac.codestado)                    
    inner join vw_relacion_pagos pagos on (pagos.codagenciacmpr=b.codagencia and pagos.tipocmpr=b.codtipocmpr and pagos.refnumero=b.numcmprventa)
    where axe.codempresa =   2
    and b.codtipocmpr not in ('NDCLI') 
    and b.codestado in (1, 4) 
    and b.numdocumento is not null
    and b.tipopago <> 'A' 
    and b.fecharegistro >= to_date('01/11/2024 00:00:00', 'dd/mm/yyyy hh24:mi:ss') 
    and b.fecharegistro <= to_date('07/11/2024 23:59:59', 'dd/mm/yyyy hh24:mi:ss') 
    and b.identificacion not in ('9999999999','9999999999999')
    and (b.telefono is not null or b.email is not null)