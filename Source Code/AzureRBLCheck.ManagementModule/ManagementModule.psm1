#region Get-AzureRBLList
function Get-AzureRBLList
{
    [CmdletBinding()]

    Param()

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        Write-Verbose "Getting Azure RBL Lists."
        $POSTParameters = @{Code = $config.APIKey}
		try
		{
			$responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "GetRBLs") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false        
			$responce
		}
		catch
		{
			Write-Error "Error getting the RBLs. $($_.Exception.Message)";
		}
    }
    
    End
    {
    }
}
#endregion

#region Add-AzureRBLList
function Add-AzureRBLList
{
    [CmdletBinding()]

    Param
    (
        [Parameter(Mandatory=$true,
				   ValueFromPipeline = $true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
		[string]
        $Name,

        [Parameter(Mandatory=$true,
				   ValueFromPipeline = $true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
		[string]
		$FQDN
    )

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        $POSTParameters = @{Name = $Name; FQDN = $FQDN; Code = $config.APIKey}

        Write-Verbose "Adding the list $Name with FQDN $FQDN to the Azure RBL lists."
		try
		{
	        $responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "AddRBL") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false
		}
		catch
		{
			Write-Error "Error adding RBL. $($_.Exception.Message)"
		}
    }

    End
    {
    }
}
#endregion

#region Remove-AzureRBLList
function Remove-AzureRBLList
{
    [CmdletBinding(SupportsShouldProcess=$true, 
                   PositionalBinding=$false,
                   ConfirmImpact='Medium')]
    
	Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true, 
                   ValueFromRemainingArguments=$false, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]
        $FQDN
    )

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        if ($pscmdlet.ShouldProcess($FQDN, "Remove-AzureRBLList"))
        {
			$POSTParameters = @{FQDN = $FQDN; Code = $config.APIKey}

            Write-Verbose "Removing the list $FQDN from the Azure RBL lists."
			try
			{
		        $responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "RemoveRBL") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false
			}
			catch
			{
				Write-Error "Error removing RBL. $($_.Exception.Message)"
			}
        }
    }
    End
    {
    }
}
#endregion

#region Get-AzureRBLHost
function Get-AzureRBLHost
{
    [CmdletBinding()]

    Param()

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        Write-Verbose "Getting the Azure RBL hosts."
        $POSTParameters = @{Code = $config.APIKey}
		try
		{
			$responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "GetHosts") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false       
			$responce
		}
		catch
		{
			Write-Error "Error getting the hosts. $($_.Exception.Message)";
		}
    }
    
    End
    {
    }
}
#endregion

#region Add-AzureRBLHost
function Add-AzureRBLHost
{
    [CmdletBinding()]

    Param
    (
        [Parameter(Mandatory=$true,
				   ValueFromPipeline = $true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
		[string]
        $Hostname,

        [Parameter(Mandatory=$true,
				   ValueFromPipeline = $true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        [ValidateNotNullOrEmpty()]
		[string]
		$IP
    )

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        Write-Verbose "Adding the host $Hostname with IP $IP to the list of Azure RBL hosts."

        $POSTParameters = @{Hostname = $Hostname; IP = $IP; Code = $config.APIKey}

		try
		{
	        $responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "AddHost") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false
		}
		catch
		{
			Write-Error "Error adding host. $($_.Exception.Message)"
		}
    }

    End
    {
    }
}
#endregion

#region Remove-AzureRBLHost
function Remove-AzureRBLHost
{
    [CmdletBinding(SupportsShouldProcess=$true, 
                   PositionalBinding=$false,
                   ConfirmImpact='Medium')]
    
	Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true, 
                   ValueFromRemainingArguments=$false, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]
        $IP
    )

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        if ($pscmdlet.ShouldProcess($IP, "Remove-Host"))
        {
            Write-Verbose "Removing the host $IP from the Azure RBL hosts list."

			$POSTParameters = @{IP = $IP; Code = $config.APIKey}

			try
			{
		        $responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "RemoveHost") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false
			}
			catch
			{
				Write-Error "Error removing host. $($_.Exception.Message)"
			}
        }
    }
    End
    {
    }
}
#endregion

#region Check-AzureRBLHost
function Check-AzureRBLHost
{
    [CmdletBinding(SupportsShouldProcess=$true, 
                   PositionalBinding=$false,
                   ConfirmImpact='Medium')]
    
	Param
    (
        [Parameter(Mandatory=$true, 
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true, 
                   ValueFromRemainingArguments=$false, 
                   Position=0)]
        [ValidateNotNullOrEmpty()]
        [string]
        $IP
    )

    Begin
    {
        # Load the configuration
        try
        {
            $configurationPath = (Get-Module -Name "AzureRBL.ManagementModule").ModuleBase + "\Configuration.json"
            $config = Get-Content $configurationPath | ConvertFrom-Json
        }
        catch
        {
            throw "Failed to read the configuration."
        }

		# Configure TLS
		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    }

    Process
    {
        Write-Verbose "Checking the host $IP against all Azure RBL lists."

        $POSTParameters = @{IP = $IP; Code = $config.APIKey}
        
		try
		{
		    $responce = Invoke-RestMethod -Uri ($config.APIBaseUri + "CheckHost") -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false
            $responce

            if( @($responce | Where-Object {$_.IsListed -eq $true}).Count -gt 0)
            {
                Write-Warning "Server $IP is listed at least on one RBL"
            }
        }
		catch
		{
		    Write-Error "Error checking host. $($_.Exception.Message)"
	    }
    }
    End
    {
    }
}
#endregion
