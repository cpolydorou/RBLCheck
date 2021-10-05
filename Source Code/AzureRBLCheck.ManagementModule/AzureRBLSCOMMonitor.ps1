#region Script Information
<#
    Name    : AzureRBLSCOMMonitor.ps1
    Author  : Christos Polydorou (christos.polydorou@hotmail.com)
    Date    : Jan 22, 2019
    Purpose : SCOM will use this script in order to call an Azure Function
              and check if a host is listed in any RBLs
    Returns : <0 - Error calling the function
               0 - Host is not listed
              >0 - Error calling the function
    Version : 1.1 - Jan 23, 2019 - Added domain check support
              1.0 - Jan 22, 2019 - Initial script 
#>
#endregion

#region Parameters
[cmdletBinding()]
[OutputType([int])]
Param
(
    [Parameter( Mandatory = $true,
                Position = 0,
                ParameterSetName = "Host")]
    [string]
    $IP,

    [Parameter( Mandatory = $true,
                Position = 0,
                ParameterSetName = "Domain")]
    [string]
    $Domain

)
#endregion

#region Configuration

# The url of the function
if($IP)
{
    $functionURL = "https://devazurerblcheck.azurewebsites.net/api/CheckHost"
}
else
{
    $functionURL = "https://devazurerblcheck.azurewebsites.net/api/CheckDomain"
}
# The key for the function
$functionKey = "T8gBT0lw/aQ2IxizaweNrJKaAFxUDZtuYsZUCbDpCQwqmgbTiDrrLg=="

# The value to return
[int]$ListedRBLCount = 0

# Configure TLS
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

#endregion

#region Logic

# Create the function parameters
if($IP)
{
    $POSTParameters = @{IP = $IP; Code = $functionKey}
}
else
{
    $POSTParameters = @{Name = $Domain; Code = $functionKey}
}

# Call the function
if($IP)
{
    Write-Verbose "Calling the Azure Function for host $IP"
}
else
{
    Write-Verbose "Calling the Azure Function for domain $Domain"
}

try
{
    $responce = Invoke-RestMethod -Uri $functionURL -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false        
}
catch
{
    Write-Error "Error getting the RBLs. $($_.Exception.Message)"
    return -3
}

# Process the result
Write-Verbose "Processing the function output."
foreach($RBLCheck in $responce)
{
    # If the server or domain is listed, return 2
    if($RBLCheck.IsListed -eq $true)
    {
        if($IP)
        {
            Write-Verbose "The host is listed on $($RBLCheck.RBL)."
        }
        else
        {
            Write-Verbose "The domain is listed on $($RBLCheck.RBL)."
        }

        $ListedRBLCount++
    }
}    

# Return the result
if($ListedRBLCount -gt 0)
{
    # The server/domain is listed
    if($IP)
    {
        Write-Verbose "The host is listed at $ListedRBLCount lists."
    }
    else
    {
        Write-Verbose "The domain is listed at $ListedRBLCount lists."
    }
}
else
{
    # The server/domain is not listed, return 1
    if($IP)
    {
        Write-Verbose "The host is not listed."
    }
    else
    {
        Write-Verbose "The domain is not listed."
    }
}

return $ListedRBLCount

#endregion