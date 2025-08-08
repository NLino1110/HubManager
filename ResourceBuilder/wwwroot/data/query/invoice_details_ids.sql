select codagencia,
    codtipocmpr,
    numcmprventa,
    NUMCMPRVENTADET,
    numdocumento,
    codalterno,
    precio,
    ROUND(sum(CANT),4) CANT, 
    sum(TOTAL) TOTAL
    from (
            select axe.codagencia, axe.nomagencia, 
            b.codtipocmpr,
            b.numcmprventa,
            a.NUMCMPRVENTADET,
            b.fecharegistro,
            b.numdocumento,
            substr(b.numdocumento,0,3) estab,
            substr(b.numdocumento,5,3) ptoemi,
            substr(b.numdocumento,9,9) secuencial,
            b.codcliente, 
            b.identificacion,
            b.tipopedido, 
            b.concepto,
            a.CODARTICULO, 
            pro.CODALTERNO,             
            to_char(b.fecharegistro, 'yyyy') anio, 
            to_char(b.fecharegistro, 'mm') mes,                             
            a.precio,            
            decode(co.tipomvto, 'E', (a.cantidad * pro.factorconversionarticulo) * -1, a.cantidad * pro.factorconversionarticulo) CANT,             
            (case when a.porcdescuento = 100 then decode(co.tipomvto, 'E', (a.cantidad * pro.factorconversionarticulo) * -1, a.cantidad * pro.factorconversionarticulo) else 0 end) CANT_BONIFICA, 
            0 CANT_NETA, 
            decode(co.tipomvto, 'E', (a.costo * a.cantidad) * -1, a.costo * a.cantidad) COSTO_VTA,
    decode(co.tipomvto, 'E', (a.subtotal) * -1, a.subtotal) SUBTOTAL, 
    decode(co.tipomvto, 'E', (a.descuento) * -1, a.descuento) DESCUENTO, 
    decode(co.tipomvto, 'E', (a.subtotal - a.descuento) * -1, a.subtotal - a.descuento) VTA_NETA, 
    decode(co.tipomvto, 'E', (a.impuesto) * -1, a.impuesto) IMPUESTO, 
    decode(co.tipomvto, 'E', (nvl(a.compensoli, 0)) * -1, nvl(a.compensoli, 0)) COMPEN_SOLI, 
    decode(co.tipomvto, 'E', (a.total) * -1, a.total) TOTAL 
    from faccmprventadet a 
    inner join faccmprventa b 
        on(a.codagencia = b.codagencia 
        and a.codtipocmpr = b.codtipocmpr 
        and a.numcmprventa = b.numcmprventa
        and b.codestado in (1, 4)
        and b.numdocumento is not null
        and b.tipopago <> 'A'
        and b.numcmprventa in ([ids])
        and b.identificacion not in ('9999999999','9999999999999')
        and (b.telefono is not null or b.email is not null)
    ) 
    inner join vw_emp_age_axe axe on(a.codagencia = axe.codagencia) 
    inner join cnttipocmpr co on(b.codtipocmpr = co.codtipocmpr) 
    left outer join gentiposclientes tipcli on (b.codtipocliente=tipcli.codtipocliente)
    left outer join GENTIPOSNOTACREDITO tnc on (b.tiponc = tnc.tiponc) 
    inner join genestados estfac on(b.codestado = estfac.codestado) 
    inner join vw_productos_2 pro on(axe.codempresa = pro.codempresa and a.codarticulo = pro.codarticulo and pro.codclasificacion = 1)        
    where axe.codempresa =   2
    and a.codtipocmpr not in ('NDCLI')    
    and((nvl(tnc.tipo,'D')='D') or (b.codtipocmpr = 'NCCLI' and tnc.tipo='V' and tnc.considerareporte='S'))    
 ) 
 group by codagencia,codtipocmpr,numcmprventa,NUMCMPRVENTADET,numdocumento,codalterno,precio

