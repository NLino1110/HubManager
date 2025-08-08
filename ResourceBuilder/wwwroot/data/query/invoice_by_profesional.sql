SELECT DISTINCT
        fid.Tipotarjeta Tipotarjeta,
        GCA.FECHACAMBIOESTADO FECHAAPROB,
        FAC.NOMBRESCLIENTE, 
        FAC.APELLIDOSCLIENTE, 
        fid.EMAIL, 
        fid.FECHANACIMIENTO,
        fid.Sexo SEXO,
        FAC.IDENTIFICACION, 
        fid.TELEFONO1,
        fid.CELULAR,
        fid.DOMICILIO,       
        NOMCIUDAD, 
        FAC.CODCIUDAD, 
        NOMPROVINCIA, 
        fid.Nombrecomercial,
        fid.DIRECCIONCOMERCIAL,
        fid.REFERENCIACOMERCIAL,
        fid.NOMBREINSTITUCION,
        fid.DIRECCIONINSTITUCION,
        fid.TELEFONOINSTITUCION,
        fid.TITULOACADEMICO,
        fid.FECHAGRADUACION,
        VEN.codtipovendedor,
        CASE VEN.codtipovendedor 
            WHEN 3 THEN 'WEB'
            ELSE 'TIENDA' 
        END TIPOCOMPRA,
        TCLI.Descripcion TIPOCLIENTE
    FROM faccmprventa FAC
    JOIN genagencias age ON age.codagencia = fac.codagencia
    JOIN cxcdocumento CXCD ON CXCD.Codagencia = fac.codagencia 
        AND CXCD.Codtipocmpr = fac.codtipocmpr 
        AND CXCD.REFNUMERO = FAC.NUMCMPRVENTA
    JOIN genvendedores VEN ON VEN.Codempresa = age.codempresa 
        AND VEN.Codvendedor = FAC.CODVENDEDOR
    JOIN Gentiposclientes TCLI ON FAC.Codtipocliente = TCLI.CODTIPOCLIENTE
    JOIN vw_pais_prov_ciudad VWC ON FAC.CODCIUDAD = VWC.codciudad
    JOIN genclienteaprobacion GCA ON GCA.IDENTIFICACION = FAC.Identificacion 
        AND GCA.codestado = 52
    JOIN clientesfidelizacion fid ON fid.IDENTIFICACION = FAC.IDENTIFICACION
    WHERE         
        FAC.fecharegistro >= :date_ini
        AND FAC.fecharegistro <= :date_end
        AND fid.Tipotarjeta IS NOT NULL