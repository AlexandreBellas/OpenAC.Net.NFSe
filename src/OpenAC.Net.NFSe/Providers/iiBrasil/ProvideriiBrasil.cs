// ***********************************************************************
// Assembly         : OpenAC.Net.NFSe
// Author           : Rafael Dias
// Created          : 07-28-2017
//
// Last Modified By : Rafael Dias
// Last Modified On : 07-28-2017
// ***********************************************************************
// <copyright file="ProviderBetha2.cs" company="OpenAC .Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2014 - 2022 Projeto OpenAC .Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using OpenAC.Net.Core.Extensions;
using OpenAC.Net.NFSe.Configuracao;

namespace OpenAC.Net.NFSe.Providers
{
    // verificar se o extends procede
    // verificar métodos LoadXYZ e PrepararXYZ
    internal sealed class ProvideriiBrasil : ProviderABRASF200
    {
        #region Constructors

        public ProvideriiBrasil(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "iiBrasil";
            Versao = VersaoNFSe.ve204;
        }

        #endregion Constructors

        #region Methods

        #region Protected Methods

        #region Services

        /// <inheritdoc />
        protected override void AssinarConsultarNFSeRps(RetornoConsultarNFSeRps retornoWebservice)
        {
            string xml = retornoWebservice.XmlEnvio
                .Replace("<ConsultarNfseRpsEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\">", "")
                .Replace("</ConsultarNfseRpsEnvio>", "");

            string firstFilter = Regex.Replace(xml, "/[^\x20-\x7E]+/g", "");
            string secondFilter = Regex.Replace(firstFilter, "/[ ]+/g", "");

            byte[] data = Encoding.UTF8.GetBytes($"{secondFilter}{this.Configuracoes.PrestadorPadrao.Token}");
            StringBuilder sb = new StringBuilder();

            using (SHA512 shaM = new SHA512Managed())
            {
                byte[] hash = shaM.ComputeHash(data);
                foreach (byte b in hash)
                {
                    sb.Append($"{b:x2}");
                }
            }

            string integridade = sb.ToString();

            XNamespace xmlns = XNamespace.Get("http://www.abrasf.org.br/nfse.xsd");
            var doc = XDocument.Parse(retornoWebservice.XmlEnvio);
            doc.Root.AddChild(new XElement(xmlns + "Integridade", integridade, ""));
            retornoWebservice.XmlEnvio = doc.ToString();
        }

        #endregion Services

        protected override IServiceClient GetClient(TipoUrl tipo) => new iiBrasilServiceClient(this, tipo);

        protected override string GetSchema(TipoUrl tipo) => "schema_nfse_v1_IIBR.xsd";

        #endregion Protected Methods

        #endregion Methods
    }
}
