﻿
namespace Anycmd.Ac.ViewModels.Identity.AccountViewModels
{
    using ViewModel;

    /// <summary>
    /// 
    /// </summary>
    public class GetPlistAccounts : GetPlistResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool? IncludeDescendants { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CatalogCode { get; set; }
    }
}
