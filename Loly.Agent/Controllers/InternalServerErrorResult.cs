using System;
using System.Net;
using Loly.Agent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Loly.Agent.Controllers
{
    public class InternalServerErrorResult : ObjectResult
    {
        public InternalServerErrorResult(string code, string message, Severity severity) : base (new Error() { Code = code, Message = message, Severity = severity })
        {
            this.StatusCode = 500;
        }
    }
    
}