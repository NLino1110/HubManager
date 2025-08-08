select codagencia,
    codtipocmpr,
    numcmprventa,
    numdocumento,
    codalterno,
    precio,
    sum(CANT) CANT, 
    sum(TOTAL) TOTAL
    from (
            select axe.codagencia, axe.nomagencia, 
            b.codtipocmpr,b.numcmprventa,b.fecharegistro,
            b.numdocumento,
            substr(b.numdocumento,0,3) estab,
            substr(b.numdocumento,5,3) ptoemi,
            substr(b.numdocumento,9,9) secuencial,
            b.codvendedor, vend.apellidos || ' ' || vend.nombres nomvendedor, 
            vend.codtipovendedor, tvend.descripcion nomtipovendedor, 
            b.codcliente, 
            cli.nomcliente, 
            cli.nombrecomercial, 
            b.identificacion,

            cli.domicilio, 
            cli.codregdom, cli.nomregdom, cli.codprovdom,cli.nomprovdom,cli.codciudom, cli.nomciudom, 
            cli.telefono1,
            cli.telefono2, 
            cli.fecha_creacion fecha_creacion_cliente,
            cli.codempresaempl,
            cli.codempleado,
            b.tipopedido, 
            b.concepto,
            (case when b.concepto is null then (case when b.codvendedor=696 then 'Venta Web' else CAST(tvend.descripcion AS VARCHAR2(30)) end) 
                else null end) nomconcepto,
            nvl(tipcli.codtipocliente,clidirec.codtipocliente) codtipoclientevta,
            nvl(tipcli.descripcion,clidirec.NOMTIPOCLIENTE) dsctipoclientevta,
            clidirec.codtipocliente codtipoclienteloc,clidirec.NOMTIPOCLIENTE dsctipoclienteloc , 
            clidirec.direccion direccionloc, clidirec.direccioncorta direccioncortaloc, 
            clidirec.CODREGION codregloc, clidirec.NOMREGION nomregloc, clidirec.CODPROVINCIA codprovloc,clidirec.NOMPROVINCIA nomprovloc, 
            clidirec.CODCIUDAD codciuloc, clidirec.NOMCIUDAD nomciuloc, clidirec.TELEFONO1 telefonolocal1,clidirec.TELEFONO2 telefonolocal2, 
            a.CODARTICULO, pro.CODALTERNO, pro.dsc_item, pro.CODUNIDADMEDIDA, pro.CODUNIDADPRESENTACION, 
            pro.codmarca, pro.dsc_marca nommarca, pro.codlinea, pro.dsc_linea nomlinea, pro.codsublinea, pro.dsc_sublinea nomsublinea, pro.codsublineados, pro.dsc_sublineasdos nomsublineasdos, 
            pro.codgrupotipo, pro.dsc_grupotipo nomgrupotipo, pro.codusoaplicamat, pro.dsc_usoaplicamat nomusoaplicamat, 
            pro.codproveedor, pro.nomproveedor, pro.origproducto, pro.fec_crea fecha_creacion_producto, pro.medidapeso, 
        pro.dsc_liquidar, pro.dsc_remate, pro.pms_reabasto, pro.pms_resurtido,
            to_char(b.fecharegistro, 'yyyy') anio, 
            to_char(b.fecharegistro, 'mm') mes,                             
            a.precio,
            decode(co.tipomvto, 'E', (a.cantidad * equivund.FACTORCONVERSION) * -1, a.cantidad * equivund.FACTORCONVERSION) CANT, 
            (case when a.porcdescuento = 100 then decode(co.tipomvto, 'E', (a.cantidad * equivund.FACTORCONVERSION) * -1, a.cantidad * equivund.FACTORCONVERSION) else 0 end) CANT_BONIFICA, 
            0 CANT_NETA, 
            decode(co.tipomvto, 'E', (a.costo * a.cantidad) * -1, a.costo * a.cantidad) COSTO_VTA,
    decode(co.tipomvto, 'E', (a.subtotal) * -1, a.subtotal) SUBTOTAL, 
    decode(co.tipomvto, 'E', (a.descuento) * -1, a.descuento) DESCUENTO, 
    decode(co.tipomvto, 'E', (a.subtotal - a.descuento) * -1, a.subtotal - a.descuento) VTA_NETA, 
    decode(co.tipomvto, 'E', (a.impuesto) * -1, a.impuesto) IMPUESTO, 
    decode(co.tipomvto, 'E', (nvl(a.compensoli, 0)) * -1, nvl(a.compensoli, 0)) COMPEN_SOLI, 
    decode(co.tipomvto, 'E', (a.total) * -1, a.total) TOTAL 
    from faccmprventadet a 
    inner join faccmprventa b on(a.codagencia = b.codagencia and a.codtipocmpr = b.codtipocmpr and a.numcmprventa = b.numcmprventa) 
    inner join vw_emp_age_axe axe on(a.codagencia = axe.codagencia) 
    inner join cnttipocmpr co on(b.codtipocmpr = co.codtipocmpr) 
    left outer join vw_cliente_direcciones clidirec on (axe.codempresa = clidirec.codempresa and b.codcliente = clidirec.codcliente and b.coddireccion = clidirec.coddireccion) 
    left outer join gentiposclientes tipcli on (b.codtipocliente=tipcli.codtipocliente)
    inner join vw_emp_cli_cxe_tcl_ven cli on(axe.codempresa = cli.codempresacli and b.codcliente = cli.codcliente) 
    inner join genvendedores vend on(b.codempresavend = vend.codempresa and b.codvendedor = vend.codvendedor) 
    inner join gentipovendedor tvend on(vend.codtipovendedor = tvend.codtipovendedor)
    left outer join GENTIPOSNOTACREDITO tnc on (b.tiponc = tnc.tiponc) 
    inner join genestados estfac on(b.codestado = estfac.codestado) 
    inner join vw_productos_2 pro on(axe.codempresa = pro.codempresa and a.codarticulo = pro.codarticulo) 
    inner join genequivalenciaunimedidas equivund on(a.codunidadmedida = equivund.codunidadorigen and pro.codunidadmedida = equivund.codunidaddestino) 
    where axe.codempresa =   2
    and a.codtipocmpr not in ('NDCLI') 
    and b.codestado in (1, 4) 
    and b.numdocumento is not null
    and b.tipopago <> 'A'
    and b.fecharegistro >= to_date('08/11/2024 00:00:00', 'dd/mm/yyyy hh24:mi:ss') 
    and b.fecharegistro <= to_date('08/11/2024 23:59:59', 'dd/mm/yyyy hh24:mi:ss') 
    and((pro.codclasificacion = 1 and nvl(tnc.tipo,'D')='D') or (b.codtipocmpr = 'NCCLI' and tnc.tipo='V' and tnc.considerareporte='S'))
    and b.identificacion not in ('9999999999','9999999999999')
    and (b.telefono is not null or b.email is not null)                    
 ) 
 group by codagencia,codtipocmpr,numcmprventa,numdocumento,codalterno,precio
                         
