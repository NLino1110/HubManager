select 
(case when codcliente in (19271, 20279, 19489) then decode(codcliente,19271,'dg_vtex',20279,'dg_rappi',19489,'dg_ws') else (decode(codtipovendedor,1,'point_sale','dg_external')) end) origin,
codagencia || codtipocmpr || numcmprventa customhash,
codagencia,codtipocmpr,numcmprventa,codcliente,tipoidentificacion,
        apellidoscliente,nombrescliente,direccion,telefono,email,                     
        identificacion,estab,ptoemi,secuencial,numdocumento,
        fecharegistro fecharegistrocrea,
        fecharegistro fecharegistroapro,
        nomagencia,dsctipoclientevta,codvendedor,nomvendedor,null,
        sum(SUBTOTAL) SUBTOTAL, 
        sum(DESCUENTO) DESCUENTO, 
        sum(IMPUESTO) IMPUESTO, 
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
                b.tipoidentificacion,
                b.identificacion,
                b.apellidoscliente,
                b.nombrescliente,
                b.direccion,
                b.telefono,
                b.email,
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
                nvl(tipcli.codtipocliente,cli.codtipocliente) codtipoclientevta,
                nvl(tipcli.descripcion,cli.dsctipocliente) dsctipoclientevta,
                cli.codtipocliente codtipoclienteloc,
                cli.dsctipocliente dsctipoclienteloc , 
                cli.direccionlocal direccionloc, 
                cli.direccioncorta direccioncortaloc, 
                cli.codregdom codregloc, 
                cli.nomregdom nomregloc, 
                cli.codprovdom codprovloc,
                cli.nomprovdom nomprovloc, 
                cli.codciudom codciuloc, 
                cli.nomciudom nomciuloc, 
                cli.TELEFONO1 telefonolocal1,
                cli.TELEFONO2 telefonolocal2, 
                a.CODARTICULO,                 
                to_char(b.fecharegistro, 'yyyy') anio, 
                to_char(b.fecharegistro, 'mm') mes,                 
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
                   left outer join gentiposclientes tipcli on (b.codtipocliente=tipcli.codtipocliente) 
                   inner join vw_emp_cli_cxe_tcl_ven cli on(axe.codempresa = cli.codempresacli and b.codcliente = cli.codcliente) 
                   inner join genvendedores vend on(b.codempresavend = vend.codempresa and b.codvendedor = vend.codvendedor) 
                   inner join gentipovendedor tvend on(vend.codtipovendedor = tvend.codtipovendedor) 
                   left outer join GENTIPOSNOTACREDITO tnc on (b.tiponc = tnc.tiponc) 
                   inner join genestados estfac on(b.codestado = estfac.codestado)
                   inner join vw_productos_2 pro on(axe.codempresa = pro.codempresa and a.codarticulo = pro.codarticulo and pro.codclasificacion = 1) 
                 where axe.codempresa = 2
                    and a.codtipocmpr not in ('NDCLI')
                    and b.codestado in (1, 4) 
                    and b.numdocumento is not null
                    and b.tipopago <> 'A' 
                    and b.fecharegistro >= :date_ini
                    and b.fecharegistro <= :date_end
                    and((nvl(tnc.tipo,'D')='D') or (b.codtipocmpr = 'NCCLI' and tnc.tipo='V' and tnc.considerareporte='S'))
                    and b.identificacion not in ('9999999999','9999999999999')
                    and (b.telefono is not null or b.email is not null)                    
 )
 group by codagencia,codtipocmpr,numcmprventa,codcliente,tipoidentificacion,codtipovendedor,nomtipovendedor,
                     apellidoscliente,nombrescliente,direccion,telefono,email, 
                     identificacion,estab,ptoemi,secuencial,numdocumento,fecharegistro,fecharegistro,
                     nomagencia,dsctipoclientevta,codvendedor,nomvendedor
                         
