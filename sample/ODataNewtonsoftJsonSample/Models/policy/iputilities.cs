namespace Microsoft.Naas.Infra.Utilities.IpUtilities;

using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

public static class IpUtilities
{
    private const string _iPv4Regex = $"{_iPv4NonMaskedRegex}(\\/(3[0-2]|[1-2][0-9]|[0-9]))?$"; // mask (0-32) or without mask.
    private const string _iPv4NonMaskedRegex =
        "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}" + // first tree octets(values of each octets is (0-255).
        "([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])"; // last octet - finished without "."

    private const char _ipSubnetDelimeter = '/';
    private const char _ipOctetDelimiter = '.';
    private const char _zeroChar = '0';
    private const int _numberOfIpv4Octets = 4;
    private const int _maxValueOfIpOctet = 255;
    private const int _maxValueOfIpSubnet = 32;
    private const int _ipSubnetPartsCount = 2;

    public static bool IsValidIpv4AddressCIDRNotation(string ipAddressValue) =>
        !string.IsNullOrWhiteSpace(ipAddressValue) &&
        Regex.IsMatch(ipAddressValue, _iPv4Regex);

    public static bool IsValidIpv4NonMaskedAddressCIDRNotation(string ipAddressValue) =>
        !string.IsNullOrWhiteSpace(ipAddressValue) &&
        Regex.IsMatch(ipAddressValue, _iPv4NonMaskedRegex);

    public static bool IsValidIpv4Address(string ipv4Address)
    {
        if (string.IsNullOrWhiteSpace(ipv4Address))
        {
            return false;
        }

        IEnumerable<string> ipv4AddressOctets = ipv4Address
            .Split(_ipOctetDelimiter);

        if (ipv4AddressOctets.Count() != _numberOfIpv4Octets)
        {
            return false;
        }

        bool firstOctetCheck = true;
        foreach (string stringOctet in ipv4AddressOctets)
        {
            if (!stringOctet.All(char.IsDigit) ||
                (firstOctetCheck && stringOctet.StartsWith(_zeroChar)) ||
                !uint.TryParse(stringOctet, out uint integerOctet) ||
                integerOctet > _maxValueOfIpOctet)
            {
                return false;
            }

            firstOctetCheck = false;
        }

        return true;
    }

    public static bool IsValidIpv4SubnetAddress(string ipv4SubnetAddress)
    {
        string[] ipSubnet = ipv4SubnetAddress.Split(_ipSubnetDelimeter);
        if (ipSubnet.Length != _ipSubnetPartsCount)
        {
            return false;
        }

        string ipAddress = ipSubnet[0];
        string maskNumber = ipSubnet[1];
        if (!IsValidIpv4Address(ipAddress) || IsValidMaskValue(maskNumber))
        {
            return false;
        }

        return true;
    }

    public static bool IsEndAddressBiggerOrEqualThanBeginAddress(string beginAddress, string endAddress)
    {
        string[] ip4NumbersBeginAdderss = beginAddress.Split(_ipOctetDelimiter);
        string[] ip4NumbersEndAddress = endAddress.Split(_ipOctetDelimiter);

        for (int i = 0; i < ip4NumbersBeginAdderss.Length; i++)
        {
            if (short.Parse(ip4NumbersBeginAdderss[i]) > short.Parse(ip4NumbersEndAddress[i]))
            {
                return false;
            }

            if (short.Parse(ip4NumbersBeginAdderss[i]) < short.Parse(ip4NumbersEndAddress[i]))
            {
                return true;
            }
        }

        return true;
    }

    public static uint MapIPAddressToUintBigEndian(IPAddress address)
    {
        byte[] ipBytes = address.GetAddressBytes();
        // most significant byte is the first byte in the array( the byte in index 0 )
        uint ipAsUint = BinaryPrimitives.ReadUInt32BigEndian(ipBytes);
        return ipAsUint;
    }

    private static bool IsValidMaskValue(string maskNumber)
    {
        return !maskNumber.All(char.IsDigit) ||
               !uint.TryParse(maskNumber, out uint integerSubnet) ||
               integerSubnet > _maxValueOfIpSubnet;
    }
}