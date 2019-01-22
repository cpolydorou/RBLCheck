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
    Version : 1.0 - Jan 22, 2019 - Initial script 
#>
#endregion

#region Parameters
[cmdletBinding()]
[OutputType([int])]
Param
(
    [Parameter( Mandatory = $true,
                Position = 0)]
    [string]
    $IP
)
#endregion

#region Configuration

# The url of the function
$functionURL = "https://azurerbl.azurewebsites.net/api/CheckHost"

# The key for the function
$functionKey = "T8gBT0lw/aQ2IxizaweNrJKaAFxUDZtuYsZUCbDpCQwqmgbTiDrrLg=="

# The value to return
[int]$ListedRBLCount = 0

# Configure TLS
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

#endregion

#region Logic

# Create the function parameters
$POSTParameters = @{IP = $IP; Code = $functionKey}

# Call the function
Write-Verbose "Calling the Azure Function for host $IP"
try
{
    $responce = Invoke-RestMethod -Uri $functionURL -Method GET -UseBasicParsing -Body $POSTParameters -ErrorAction Stop -Verbose:$false        
}
catch
{
    Write-Error "Error getting the RBLs. $($_.Exception.Message)"
    return 3
}

# Process the result
Write-Verbose "Processing the function output."
foreach($RBLCheck in $responce)
{
    # If the server is listed, return 2
    if($RBLCheck.IsListed -eq $true)
    {
        Write-Verbose "The host is listed on $($RBLCheck.RBL)."
        $ListedRBLCount++
    }
}    

# Return the result
if($ListedRBLCount -gt 0)
{
    # The server is listed
    Write-Verbose "The host is listed at $ListedRBLCount lists."
}
else
{
    # The server is not listed, return 1
    Write-Verbose "The host is not listed."
}

return $ListedRBLCount

#endregion