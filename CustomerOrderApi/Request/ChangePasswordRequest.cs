﻿namespace CustomerOrderApi.Request
{
    public class ChangePasswordRequest
    {
        
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }      
    }
}
