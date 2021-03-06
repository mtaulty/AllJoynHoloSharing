//-----------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
//
//   For more information, see: http://go.microsoft.com/fwlink/?LinkID=623246
// </auto-generated>
//-----------------------------------------------------------------------------
#include "pch.h"

using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Devices::AllJoyn;
using namespace Windows::Foundation;
using namespace com::mtaulty::AJHoloServer;

void AJHoloServerLegacySignals::Initialize(_In_ alljoyn_busobject busObject, _In_ alljoyn_sessionid sessionId)
{
    m_busObject = busObject;
    m_sessionId = sessionId;

    auto interfaceDefinition = alljoyn_busattachment_getinterface(alljoyn_busobject_getbusattachment(busObject), "com.mtaulty.AJHoloServer");
    alljoyn_interfacedescription_getmember(interfaceDefinition, "WorldAnchorAdded", &m_memberWorldAnchorAdded);
    alljoyn_interfacedescription_getmember(interfaceDefinition, "HologramAdded", &m_memberHologramAdded);
    alljoyn_interfacedescription_getmember(interfaceDefinition, "HologramRemoved", &m_memberHologramRemoved);
}

void AJHoloServerLegacySignals::WorldAnchorAdded(_In_ Platform::String^ interfaceMemberAnchorId)
{
    if (nullptr == m_busObject)
    {
        return;
    }

    size_t argCount = 1;
    alljoyn_msgarg arguments = alljoyn_msgarg_array_create(argCount);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 0), "s", interfaceMemberAnchorId);
    
    alljoyn_busobject_signal(
        m_busObject, 
        NULL,  // Generated code only supports broadcast signals.
        m_sessionId,
        m_memberWorldAnchorAdded,
        arguments,
        argCount, 
        0, // A signal with a TTL of 0 will be sent to every member of the session, regardless of how long it takes to deliver the message
        ALLJOYN_MESSAGE_FLAG_GLOBAL_BROADCAST, // Broadcast to everyone in the session.
        NULL); // The generated code does not need the generated signal message

    alljoyn_msgarg_destroy(arguments);
}

void AJHoloServerLegacySignals::CallWorldAnchorAddedReceived(_In_ AJHoloServerLegacySignals^ sender, _In_ AJHoloServerWorldAnchorAddedReceivedEventArgs^ args)
{
    WorldAnchorAddedReceived(sender, args);
}

void AJHoloServerLegacySignals::HologramAdded(_In_ Platform::String^ interfaceMemberAnchorId, _In_ Platform::String^ interfaceMemberHoloId, _In_ Platform::String^ interfaceMemberHoloTypeName, _In_ AJHoloServerPosition^ interfaceMemberPosition)
{
    if (nullptr == m_busObject)
    {
        return;
    }

    size_t argCount = 4;
    alljoyn_msgarg arguments = alljoyn_msgarg_array_create(argCount);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 0), "s", interfaceMemberAnchorId);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 1), "s", interfaceMemberHoloId);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 2), "s", interfaceMemberHoloTypeName);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 3), "(ddd)", interfaceMemberPosition);
    
    alljoyn_busobject_signal(
        m_busObject, 
        NULL,  // Generated code only supports broadcast signals.
        m_sessionId,
        m_memberHologramAdded,
        arguments,
        argCount, 
        0, // A signal with a TTL of 0 will be sent to every member of the session, regardless of how long it takes to deliver the message
        ALLJOYN_MESSAGE_FLAG_GLOBAL_BROADCAST, // Broadcast to everyone in the session.
        NULL); // The generated code does not need the generated signal message

    alljoyn_msgarg_destroy(arguments);
}

void AJHoloServerLegacySignals::CallHologramAddedReceived(_In_ AJHoloServerLegacySignals^ sender, _In_ AJHoloServerHologramAddedReceivedEventArgs^ args)
{
    HologramAddedReceived(sender, args);
}

void AJHoloServerLegacySignals::HologramRemoved(_In_ Platform::String^ interfaceMemberHoloId)
{
    if (nullptr == m_busObject)
    {
        return;
    }

    size_t argCount = 1;
    alljoyn_msgarg arguments = alljoyn_msgarg_array_create(argCount);
    (void)TypeConversionHelpers::SetAllJoynMessageArg(alljoyn_msgarg_array_element(arguments, 0), "s", interfaceMemberHoloId);
    
    alljoyn_busobject_signal(
        m_busObject, 
        NULL,  // Generated code only supports broadcast signals.
        m_sessionId,
        m_memberHologramRemoved,
        arguments,
        argCount, 
        0, // A signal with a TTL of 0 will be sent to every member of the session, regardless of how long it takes to deliver the message
        ALLJOYN_MESSAGE_FLAG_GLOBAL_BROADCAST, // Broadcast to everyone in the session.
        NULL); // The generated code does not need the generated signal message

    alljoyn_msgarg_destroy(arguments);
}

void AJHoloServerLegacySignals::CallHologramRemovedReceived(_In_ AJHoloServerLegacySignals^ sender, _In_ AJHoloServerHologramRemovedReceivedEventArgs^ args)
{
    HologramRemovedReceived(sender, args);
}

