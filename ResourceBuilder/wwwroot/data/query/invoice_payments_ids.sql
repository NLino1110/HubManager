select b.codagencia,
    b.codtipocmpr,
    b.numcmprventa,
    b.numdocumento,
    pagos.nummovimientodet,
    pagos.codformapago,
    pagos.codtarjeta,
    pagos.codbanco,
    pagos.numtarjetacredito,
    pagos.cuotas_diferido,
    pagos.tipo_diferido,
    pagos.valor
    from faccmprventa b 
    inner join vw_emp_age_axe axe on(b.codagencia = axe.codagencia and axe.codempresa =   2) 
    inner join cnttipocmpr co on(b.codtipocmpr = co.codtipocmpr)
    left outer join GENTIPOSNOTACREDITO tnc on (b.tiponc = tnc.tiponc) 
    inner join genestados estfac on(b.codestado = estfac.codestado)                    
    inner join vw_relacion_pagos pagos on (pagos.codagenciacmpr=b.codagencia and pagos.tipocmpr=b.codtipocmpr and pagos.refnumero=b.numcmprventa)
    where 
    b.codtipocmpr not in ('NDCLI') 
    and b.codestado in (1, 4) 
    and b.numdocumento is not null
    and b.tipopago <> 'A' 
    and b.numcmprventa in ([ids])
    and b.identificacion not in ('9999999999','9999999999999')
    and (b.telefono is not null or b.email is not null)
